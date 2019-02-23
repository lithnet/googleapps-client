using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lithnet.GoogleApps
{
    public class ClassroomRequestFactory
    {
        private readonly BaseClientServicePool<ClassroomService> classroomServicePool;

        public CourseStudentRequestFactory StudentFactory { get; private set; }

        public CourseTeacherRequestFactory TeacherFactory { get; private set; }

        protected const string CourseState_Archived = "ARCHIVED";

        public int RetryCount { get; set; } = 5;

        public ClassroomRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
        {
            this.classroomServicePool = new BaseClientServicePool<ClassroomService>(poolSize, () =>
            {
                ClassroomService x = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(scopes)),
                    ApplicationName = "LithnetGoogleAppsLibrary",
                    GZipEnabled = !Settings.DisableGzip,
                    Serializer = new GoogleJsonSerializer(),
                    DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                });

                x.HttpClient.Timeout = Settings.DefaultTimeout;
                return x;
            });

            this.StudentFactory = new CourseStudentRequestFactory(this.classroomServicePool);
            this.TeacherFactory = new CourseTeacherRequestFactory(this.classroomServicePool);
        }

        public Course GetCourse(string id)
        {
            using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take())
            {
                CoursesResource.GetRequest request = new CoursesResource.GetRequest(connection.Item, id);
                return request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
            }
        }

        public IEnumerable<GoogleCourse> GetCourses(string customerID, bool getStudents, bool getTeachers, bool skipMembersForArchived, int memberThreads)
        {
            BlockingCollection<GoogleCourse> completedCourses = new BlockingCollection<GoogleCourse>();

            Task t = new Task(() => this.PopulateCourses(customerID, null, getStudents, getTeachers, skipMembersForArchived, completedCourses, memberThreads));
            t.Start();

            foreach (GoogleCourse course in completedCourses.GetConsumingEnumerable())
            {
                Debug.WriteLine($"Course enumeration completed: {course.Course.Id}");
                yield return course;
            }
        }

        private void PopulateCourses(string customerID, string userKey, bool getStudents, bool getTeachers, bool skipMembersForArchived, BlockingCollection<GoogleCourse> completedCourses, int memberThreads)
        {
            BlockingCollection<GoogleCourse> studentsQueue = new BlockingCollection<GoogleCourse>();
            BlockingCollection<GoogleCourse> teachersQueue = new BlockingCollection<GoogleCourse>();
            BlockingCollection<GoogleCourse> incomingCourses = new BlockingCollection<GoogleCourse>();

            List<Task> tasks = new List<Task>();

            if (getStudents)
            {
                Task t = new Task(() => this.ConsumeStudentsQueue(memberThreads, studentsQueue, completedCourses));
                t.Start();
                tasks.Add(t);
            }

            if (getTeachers)
            {
                Task t = new Task(() => this.ConsumeTeachersQueue(memberThreads, teachersQueue, completedCourses));
                t.Start();
                tasks.Add(t);
            }

            Task t1 = new Task(() =>
            {
                using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take(NullValueHandling.Ignore))
                {

                    string token = null;
                    CoursesResource.ListRequest request = new CoursesResource.ListRequest(connection.Item)
                    {
                        //PageSize = 500

                    };

                    request.PrettyPrint = false;

                    do
                    {
                        request.PageToken = token;

                        ListCoursesResponse pageResults = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);

                        if (pageResults.Courses == null)
                        {
                            break;
                        }

                        if (pageResults.Courses == null)
                        {
                            break;
                        }

                        foreach (Course item in pageResults.Courses)
                        {

                            GoogleCourse c = new GoogleCourse(item);
                            c.RequiresStudents = getStudents && getMembersForCourse(c.Course, skipMembersForArchived);
                            c.RequiresTeachers = getTeachers && getMembersForCourse(c.Course, skipMembersForArchived);

                            incomingCourses.Add(c);


                            if (c.RequiresStudents)
                            {
                                studentsQueue.Add(c);
                            }

                            if (c.RequiresTeachers)
                            {
                                teachersQueue.Add(c);
                            }

                            if (!c.RequiresStudents && !c.RequiresTeachers)
                            {
                                completedCourses.Add(c);
                            }
                        }

                        token = pageResults.NextPageToken;

                    } while (token != null);

                    incomingCourses.CompleteAdding();
                    studentsQueue.CompleteAdding();
                    teachersQueue.CompleteAdding();
                }
            });

            t1.Start();
            tasks.Add(t1);
            Task.WhenAll(tasks).ContinueWith((a) => completedCourses.CompleteAdding());
        }

        private bool getMembersForCourse(Course c, bool skipMembersForArchived)
        {
            if(!skipMembersForArchived)
            {
                return true;
            }

            return c.CourseState != ClassroomRequestFactory.CourseState_Archived;
        }

        private void ConsumeStudentsQueue(int threads, BlockingCollection<GoogleCourse> courses, BlockingCollection<GoogleCourse> completedCourses)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(courses.GetConsumingEnumerable(), op, (course) =>
            {
                try
                {
                    lock (course)
                    {
                        try
                        {
                            course.Students = this.StudentFactory.GetCourseStudents(course.Course.Id);
                        }
                        catch (Exception ex)
                        {
                            course.Errors.Add(ex);
                        }
                        finally
                        {
                            course.LoadedStudents = true;
                        }

                        Debug.WriteLine($"Course students completed: {course.Course.Id}");

                        if (course.IsComplete)
                        {
                            completedCourses.Add(course);
                        }
                    }
                }
                catch (AggregateException ex)
                {
                    course.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    course.Errors.Add(ex);
                }
            });
        }

        private void ConsumeTeachersQueue(int threads, BlockingCollection<GoogleCourse> courses, BlockingCollection<GoogleCourse> completedCourses)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(courses.GetConsumingEnumerable(), op, (course) =>
            {
                try
                {
                    lock (course)
                    {
                        try
                        {
                            course.Teachers = this.TeacherFactory.GetCourseTeachers(course.Course.Id);
                        }
                        catch (Exception ex)
                        {
                            course.Errors.Add(ex);
                        }
                        finally
                        {
                            course.LoadedTeachers = true;
                        }

                        Debug.WriteLine($"Course teachers completed: {course.Course.Id}");

                        if (course.IsComplete)
                        {
                            completedCourses.Add(course);
                        }
                    }
                }
                catch (AggregateException ex)
                {
                    course.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    course.Errors.Add(ex);
                }
            });
        }

        public void Delete(string id)
        {
            using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take())
            {
                CoursesResource.DeleteRequest request = new CoursesResource.DeleteRequest(connection.Item, id);
                request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
            }
        }

        public Course Add(Course course)
        {
            Trace.WriteLine($"Getting Classroom Service for adding course {course.Id}");
            using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take())
            {
                Trace.WriteLine($"Got Service for course {course.Id}. Creating request...");
                CoursesResource.CreateRequest request = new CoursesResource.CreateRequest(connection.Item, course);
                Trace.WriteLine($"Created request for course {course.Id}. Executing...");

                // RetryEvent Aborted is here because Google Classroom API Create Course randomly seems to return 409 aborted
                // without any reason when many requests are run in parallel.
                Course c = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout | RetryEvents.Aborted, RetryCount);
                Trace.WriteLine($"Done adding course {course.Id}.");
                return c;
            }
        }

        public Course Update(Course course)
        {
            using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take())
            {
                CoursesResource.UpdateRequest request = new CoursesResource.UpdateRequest(connection.Item, course, course.Id);
                return request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
            }
        }

        public Course Patch(string id, Course item, object updateMask)
        {
            using (PoolItem<ClassroomService> connection = this.classroomServicePool.Take(NullValueHandling.Ignore))
            {
                CoursesResource.PatchRequest request = new CoursesResource.PatchRequest(connection.Item, item, item.Id);
                request.UpdateMask = updateMask;
                return request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
            }
        }

    }
}