using Google;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Requests;
using Lithnet.GoogleApps.ManagedObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using static Google.Apis.Requests.BatchRequest;

namespace Lithnet.GoogleApps
{
    public partial class CourseTeacherRequestFactory
    {
        private static string limiterName = "concurrent-classroom-teachers-requests";

        public static int BatchSize { get; set; } = 100;

        public static int ConcurrentOperationLimitDefault { get; set; } = 100;

        private readonly BaseClientServicePool<ClassroomService> classRoomServicePool;

        private readonly TimeSpan DefaultTimeout = new TimeSpan(0, 2, 0);
        public int RetryCount { get; set; } = 5;

        internal CourseTeacherRequestFactory(BaseClientServicePool<ClassroomService> classroomServicePool)
        {
            this.classRoomServicePool = classroomServicePool;
            RateLimiter.SetConcurrentLimit(CourseTeacherRequestFactory.limiterName, CourseTeacherRequestFactory.ConcurrentOperationLimitDefault);
        }

        private void WaitForGate()
        {
            RateLimiter.WaitForGate(CourseTeacherRequestFactory.limiterName);
        }

        private void ReleaseGate()
        {
            RateLimiter.ReleaseGate(CourseTeacherRequestFactory.limiterName);
        }

        public CourseTeachers GetCourseTeachers(string courseId)
        {
            CourseTeachers courseTeachers = new CourseTeachers();

            using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                CoursesResource.TeachersResource.ListRequest request = poolService.Item.Courses.Teachers.List(courseId);
                request.PrettyPrint = false;
                Trace.WriteLine($"Getting teachers from course {courseId}");

                do
                {
                    request.PageToken = token;
                    ListTeachersResponse teachers;

                    try
                    {
                        this.WaitForGate();
                        teachers = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                    }
                    finally
                    {
                        this.ReleaseGate();
                    }

                    if (teachers.Teachers != null)
                    {
                        foreach (Teacher teacher in teachers.Teachers)
                        {
                            if (!string.IsNullOrWhiteSpace(teacher.UserId))
                            {
                                courseTeachers.AddTeacher(teacher.UserId);
                            }
                        }
                    }

                    token = teachers.NextPageToken;
                } while (token != null);
            }

            Trace.WriteLine($"Returned {courseTeachers.Count} teachers in course {courseId}");

            return courseTeachers;
        }

        public void AddTeacher(string courseId, string teacherId)
        {
            this.AddTeacher(courseId, teacherId, true);
        }

        public void AddTeacher(string courseId, string teacherId, bool throwOnExistingTeacher)
        {
            Teacher teacher = new Teacher();

            teacher.UserId = teacherId;
            

            this.AddTeacher(courseId, teacher, throwOnExistingTeacher);
        }

        public void AddTeacher(string courseId, Teacher item)
        {
            this.AddTeacher(courseId, item, true);
        }

        public void AddTeacher(string courseId, Teacher item, bool throwOnExistingTeacher)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    CoursesResource.TeachersResource.CreateRequest request = poolService.Item.Courses.Teachers.Create(item, courseId);
                    Trace.WriteLine($"Adding teacher {item.UserId} to course {courseId}");
                    request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                }
            }
            catch (GoogleApiException e)
            {
                if (!throwOnExistingTeacher && this.IsExistingTeacherError(e.HttpStatusCode, e.Message))
                {
                    return;
                }

                throw;
            }
            finally
            {
                this.ReleaseGate();
            }
        }


        public void RemoveTeacher(string courseId, string userId)
        {
            this.RemoveTeacher(courseId, userId, true);
        }

        public void RemoveTeacher(string courseId, string userId, bool throwOnMissingTeacher)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    CoursesResource.TeachersResource.DeleteRequest request = poolService.Item.Courses.Teachers.Delete(courseId, userId);
                    Trace.WriteLine($"Removing teacher {userId} from course {courseId}");
                    try
                    {
                        request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                    }
                    catch (GoogleApiException e)
                    {
                        if (!throwOnMissingTeacher && this.IsMissingTeacherError(e.HttpStatusCode, e.Message))
                        {
                            return;
                        }

                        throw;
                    }
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        public void AddTeachers(string courseId, IList<Teacher> teachers, bool throwOnExistingTeacher)
        {
            if (CourseTeacherRequestFactory.BatchSize <= 1)
            {
                foreach (Teacher teacher in teachers)
                {
                    this.AddTeacher(courseId, teacher, throwOnExistingTeacher);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Teacher>> requests = new List<ClientServiceRequest<Teacher>>();

                    foreach (Teacher teacher in teachers)
                    {
                        Trace.WriteLine($"Queuing batch teacher add for {teacher.UserId} to course {courseId}");
                        requests.Add(poolService.Item.Courses.Teachers.Create(teacher, courseId));
                    }

                    this.ProcessBatches<Teacher>(courseId, !throwOnExistingTeacher, false, requests, poolService, (batchHelper) =>
                    {
                        return (content, error, i, message) =>
                        {
                            int index = batchHelper.BaseCount + i;
                            this.ProcessTeacherResponse(courseId, (teachers[index] as Teacher).UserId, batchHelper.IgnoreExistingTeacher, batchHelper.IgnoreMissingTeacher, error, message, batchHelper.RequestsToRetry, batchHelper.Request, batchHelper.FailedTeachers, batchHelper.Failures);
                        };
                    });
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        private void ProcessBatches<T>(string id, bool ignoreExistingTeacher, bool ignoreMissingTeacher, IList<ClientServiceRequest<T>> requests, PoolItem<ClassroomService> poolService, Func<ProcessBatchHelper<T>, OnResponse<CoursesResource.TeachersResource>> onResponse)
        {
            ProcessBatchHelper<T> batchHelper = new ProcessBatchHelper<T>()
            {
                FailedTeachers = new List<string>(),
                Failures = new List<Exception>(),
                IgnoreExistingTeacher = ignoreExistingTeacher,
                IgnoreMissingTeacher = ignoreMissingTeacher,
                RequestsToRetry = new Dictionary<string, ClientServiceRequest<T>>()
            };

            int batchCount = 0;

            foreach (IEnumerable<ClientServiceRequest<T>> batch in requests.Batch(CourseTeacherRequestFactory.BatchSize))
            {
                BatchRequest batchRequest = new BatchRequest(poolService.Item);
                Trace.WriteLine($"Executing teacher batch {++batchCount} for course {id}");

                foreach (ClientServiceRequest<T> request in batch)
                {
                    batchHelper.Request = request;
                    batchRequest.Queue<CoursesResource.TeachersResource>(request, onResponse.Invoke(batchHelper));
                }

                batchRequest.ExecuteWithRetryOnBackoff(poolService.Item.Name);

                batchHelper.BaseCount += CourseTeacherRequestFactory.BatchSize;
            }

            if (batchHelper.RequestsToRetry.Count > 0)
            {
                Trace.WriteLine($"Retrying {batchHelper.RequestsToRetry} teacher change requests");
            }

            foreach (KeyValuePair<string, ClientServiceRequest<T>> request in batchHelper.RequestsToRetry)
            {
                try
                {
                    request.Value.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                }
                catch (GoogleApiException e)
                {
                    if (!(ignoreMissingTeacher && this.IsMissingTeacherError(e.HttpStatusCode, e.Message)))
                    {
                        if (!(ignoreExistingTeacher && this.IsExistingTeacherError(e.HttpStatusCode, e.Message)))
                        {
                            batchHelper.Failures.Add(e);
                            batchHelper.FailedTeachers.Add(request.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    batchHelper.Failures.Add(ex);
                    batchHelper.FailedTeachers.Add(request.Key);
                }
            }

            if (batchHelper.Failures.Count == 1)
            {
                throw batchHelper.Failures[0];
            }
            else if (batchHelper.Failures.Count > 1)
            {
                throw new AggregateCourseTeacherException(id, batchHelper.FailedTeachers, batchHelper.Failures);
            }
        }

        private void ProcessTeacherResponse<T>(string id, string teacherKey, bool ignoreExistingTeacher, bool ignoreMissingTeacher, RequestError error, HttpResponseMessage message, Dictionary<string, ClientServiceRequest<T>> requestsToRetry, ClientServiceRequest<T> request, List<string> failedTeachers, List<Exception> failures)
        {
           
            string requestType = request.GetType().Name;

            if (error == null)
            {
                Trace.WriteLine($"{requestType}: Success: Teacher: {teacherKey}, Course: {id}");
                return;
            }

            string errorString = $"{error}\nFailed {requestType}: {teacherKey}\nCourse: {id}";

            Trace.WriteLine($"{requestType}: Failed: Teacher: {teacherKey}, Course: {id}\n{error}");

            if (ignoreExistingTeacher && this.IsExistingTeacherError(message.StatusCode, errorString))
            {
                return;
            }

            if (ignoreMissingTeacher && this.IsMissingTeacherError(message.StatusCode, errorString))
            {
                return;
            }

            if (ApiExtensions.IsRetryableError(message.StatusCode, errorString))
            {
                Trace.WriteLine($"Queuing {requestType} of teacher {teacherKey} from course {id} for backoff/retry");
                requestsToRetry.Add(teacherKey, request);
                return;
            }

            GoogleApiException ex = new GoogleApiException("admin", errorString)
            {
                HttpStatusCode = message.StatusCode
            };
            failedTeachers.Add(teacherKey);
            failures.Add(ex);
        }

        public void RemoveTeachers(string id, IList<string> teachers, bool throwOnMissingTeacher)
        {
            if (CourseTeacherRequestFactory.BatchSize <= 1)
            {
                foreach (string teacher in teachers)
                {
                    this.RemoveTeacher(id, teacher, throwOnMissingTeacher);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Empty>> requests = new List<ClientServiceRequest<Empty>>();

                    foreach (string teacher in teachers)
                    {
                        Trace.WriteLine($"Queuing batch teacher delete for {teacher} from course {id}");
                        requests.Add(poolService.Item.Courses.Teachers.Delete(id, teacher));
                    }

                    this.ProcessBatches(id, false, !throwOnMissingTeacher, requests, poolService, (batchHelper) =>
                    {
                        return (content, error, i, message) =>
                        {
                            int index = batchHelper.BaseCount + i;
                            this.ProcessTeacherResponse(id, teachers[index], batchHelper.IgnoreExistingTeacher, batchHelper.IgnoreMissingTeacher, error, message, batchHelper.RequestsToRetry, batchHelper.Request, batchHelper.FailedTeachers, batchHelper.Failures);
                        };
                    });
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

       
        private bool IsExistingTeacherError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.Conflict)
            {
                return true;
            }

            return false;
        }

        private bool IsMissingTeacherError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.NotFound && message.IndexOf("notFound", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}