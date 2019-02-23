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
using System.Linq;
using Google.Apis.Services;
using static Google.Apis.Requests.BatchRequest;

namespace Lithnet.GoogleApps
{
    public class CourseStudentRequestFactory
    {
        private static string limiterName = "concurrent-classroom-students-requests";

        public static int BatchSize { get; set; } = 100;

        public static int ConcurrentOperationLimitDefault { get; set; } = 100;

        private readonly BaseClientServicePool<ClassroomService> classRoomServicePool;

        private readonly TimeSpan DefaultTimeout = new TimeSpan(0, 2, 0);

        public int RetryCount { get; set; } = 5;

        internal CourseStudentRequestFactory(BaseClientServicePool<ClassroomService> classroomServicePool)
        {
            this.classRoomServicePool = classroomServicePool;
            RateLimiter.SetConcurrentLimit(CourseStudentRequestFactory.limiterName, CourseStudentRequestFactory.ConcurrentOperationLimitDefault);
        }

        private void WaitForGate()
        {
            RateLimiter.WaitForGate(CourseStudentRequestFactory.limiterName);
        }

        private void ReleaseGate()
        {
            RateLimiter.ReleaseGate(CourseStudentRequestFactory.limiterName);
        }

        public CourseStudents GetCourseStudents(string courseId)
        {
            CourseStudents courseStudents = new CourseStudents();

            using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                CoursesResource.StudentsResource.ListRequest request = poolService.Item.Courses.Students.List(courseId);
                request.PrettyPrint = false;
                Trace.WriteLine($"Getting students from course {courseId}");

                do
                {
                    request.PageToken = token;
                    ListStudentsResponse students;

                    try
                    {
                        this.WaitForGate();
                        students = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                    }
                    finally
                    {
                        this.ReleaseGate();
                    }

                    if (students.Students != null)
                    {
                        foreach (Student student in students.Students)
                        {
                            if (!string.IsNullOrWhiteSpace(student.UserId))
                            {
                                courseStudents.AddStudent(student.UserId);
                            }
                        }
                    }

                    token = students.NextPageToken;
                } while (token != null);
            }

            Trace.WriteLine($"Returned {courseStudents.Count} students in course {courseId}");

            return courseStudents;
        }

        public void AddStudent(string courseId, string studentId, string role)
        {
            this.AddStudent(courseId, studentId, true);
        }

        public void AddStudent(string courseId, string studentId, bool throwOnExistingStudent)
        {
            Student student = new Student();

            student.UserId = studentId;


            this.AddStudent(courseId, student, throwOnExistingStudent);
        }

        public void AddStudent(string courseId, Student item)
        {
            this.AddStudent(courseId, item, true);
        }

        public void AddStudent(string courseId, Student item, bool throwOnExistingStudent)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    CoursesResource.StudentsResource.CreateRequest request = poolService.Item.Courses.Students.Create(item, courseId);
                    Trace.WriteLine($"Adding student {item.UserId} to course {courseId}");
                    request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                }
            }
            catch (GoogleApiException e)
            {
                if (!throwOnExistingStudent && this.IsExistingStudentError(e.HttpStatusCode, e.Message))
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


        public void RemoveStudent(string courseId, string userId)
        {
            this.RemoveStudent(courseId, userId, true);
        }

        public void RemoveStudent(string courseId, string userId, bool throwOnMissingStudent)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    CoursesResource.StudentsResource.DeleteRequest request = poolService.Item.Courses.Students.Delete(courseId, userId);
                    Trace.WriteLine($"Removing student {userId} from course {courseId}");
                    try
                    {
                        request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                    }
                    catch (GoogleApiException e)
                    {
                        if (!throwOnMissingStudent && this.IsMissingStudentError(e.HttpStatusCode, e.Message))
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

        public void AddStudents(string courseId, IList<Student> students, bool throwOnExistingStudent)
        {
            if (CourseStudentRequestFactory.BatchSize <= 1)
            {
                foreach (Student student in students)
                {
                    this.AddStudent(courseId, student, throwOnExistingStudent);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Student>> requests = new List<ClientServiceRequest<Student>>();

                    foreach (Student student in students)
                    {
                        Trace.WriteLine($"Queuing batch student add for {student.UserId} to course {courseId}");
                        requests.Add(poolService.Item.Courses.Students.Create(student, courseId));
                    }
                    this.ProcessBatches<Student>(courseId, !throwOnExistingStudent, false, requests, poolService, (batchHelper) =>
                    {
                        return (content, error, i, message) =>
                        {
                            int index = batchHelper.BaseCount + i;
                            this.ProcessStudentResponse(courseId, (students[index] as Student).UserId, batchHelper.IgnoreExistingStudent, batchHelper.IgnoreMissingStudent, error, message, batchHelper.RequestsToRetry, batchHelper.Request, batchHelper.FailedStudents, batchHelper.Failures);
                        };
                    });
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        private void ProcessBatches<T>(string id, bool ignoreExistingStudent, bool ignoreMissingStudent, IList<ClientServiceRequest<T>> requests, PoolItem<ClassroomService> poolService, Func<CourseStudentRequestBatchHelper<T>, OnResponse<CoursesResource.StudentsResource>> onResponse)
        {
            CourseStudentRequestBatchHelper<T> batchHelper = new CourseStudentRequestBatchHelper<T>()
            {
                FailedStudents = new List<string>(),
                Failures = new List<Exception>(),
                IgnoreExistingStudent = ignoreExistingStudent,
                IgnoreMissingStudent = ignoreMissingStudent,
                RequestsToRetry = new Dictionary<string, ClientServiceRequest<T>>()
            };

            int batchCount = 0;

            foreach (IEnumerable<ClientServiceRequest<T>> batch in requests.Batch(CourseStudentRequestFactory.BatchSize))
            {
                BatchRequest batchRequest = new BatchRequest(poolService.Item);
                Trace.WriteLine($"Executing student batch {++batchCount} for course {id}");

                foreach (ClientServiceRequest<T> request in batch)
                {
                    batchHelper.Request = request;
                    batchRequest.Queue<CoursesResource.StudentsResource>(request, onResponse.Invoke(batchHelper));
                }

                batchRequest.ExecuteWithRetryOnBackoff(poolService.Item.Name);

                batchHelper.BaseCount += CourseStudentRequestFactory.BatchSize;
            }

            if (batchHelper.RequestsToRetry.Count > 0)
            {
                Trace.WriteLine($"Retrying {batchHelper.RequestsToRetry} student change requests");
            }

            foreach (KeyValuePair<string, ClientServiceRequest<T>> request in batchHelper.RequestsToRetry)
            {
                try
                {
                    request.Value.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout, RetryCount);
                }
                catch (GoogleApiException e)
                {
                    if (!(ignoreMissingStudent && this.IsMissingStudentError(e.HttpStatusCode, e.Message)))
                    {
                        if (!(ignoreExistingStudent && this.IsExistingStudentError(e.HttpStatusCode, e.Message)))
                        {
                            batchHelper.Failures.Add(e);
                            batchHelper.FailedStudents.Add(request.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    batchHelper.Failures.Add(ex);
                    batchHelper.FailedStudents.Add(request.Key);
                }
            }

            if (batchHelper.Failures.Count == 1)
            {
                throw batchHelper.Failures[0];
            }
            else if (batchHelper.Failures.Count > 1)
            {
                throw new AggregateCourseStudentException(id, batchHelper.FailedStudents, batchHelper.Failures);
            }
        }


        private void ProcessStudentResponse<T>(string id, string studentKey, bool ignoreExistingStudent, bool ignoreMissingStudent, RequestError error, HttpResponseMessage message, Dictionary<string, ClientServiceRequest<T>> requestsToRetry, ClientServiceRequest<T> request, List<string> failedStudents, List<Exception> failures)
        {

            string requestType = request.GetType().Name;

            if (error == null)
            {
                Trace.WriteLine($"{requestType}: Success: Student: {studentKey}, Course: {id}");
                return;
            }

            string errorString = $"{error}\nFailed {requestType}: {studentKey}\nCourse: {id}";

            Trace.WriteLine($"{requestType}: Failed: Student: {studentKey}, Course: {id}\n{error}");

            if (ignoreExistingStudent && this.IsExistingStudentError(message.StatusCode, errorString))
            {
                return;
            }

            if (ignoreMissingStudent && this.IsMissingStudentError(message.StatusCode, errorString))
            {
                return;
            }

            if (ApiExtensions.IsRetryableError(message.StatusCode, errorString))
            {
                Trace.WriteLine($"Queuing {requestType} of student {studentKey} from course {id} for backoff/retry");
                requestsToRetry.Add(studentKey, request);
                return;
            }

            GoogleApiException ex = new GoogleApiException("admin", errorString)
            {
                HttpStatusCode = message.StatusCode
            };
            failedStudents.Add(studentKey);
            failures.Add(ex);
        }

        public void RemoveStudents(string id, IList<string> students, bool throwOnMissingStudent)
        {
            if (CourseStudentRequestFactory.BatchSize <= 1)
            {
                foreach (string student in students)
                {
                    this.RemoveStudent(id, student, throwOnMissingStudent);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<ClassroomService> poolService = this.classRoomServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Empty>> requests = new List<ClientServiceRequest<Empty>>();

                    foreach (string student in students)
                    {
                        Trace.WriteLine($"Queuing batch student delete for {student} from course {id}");
                        requests.Add(poolService.Item.Courses.Students.Delete(id, student));
                    }

                    this.ProcessBatches(id, false, !throwOnMissingStudent, requests, poolService, (batchHelper) =>
                    {
                        return (content, error, i, message) =>
                       {
                           int index = batchHelper.BaseCount + i;
                           this.ProcessStudentResponse(id, students[index], batchHelper.IgnoreExistingStudent, batchHelper.IgnoreMissingStudent, error, message, batchHelper.RequestsToRetry, batchHelper.Request, batchHelper.FailedStudents, batchHelper.Failures);
                       };
                    });
                }

            }
            finally
            {
                this.ReleaseGate();
            }
        }

        private bool IsExistingStudentError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.Conflict)
            {
                return true;
            }

            return false;
        }

        private bool IsMissingStudentError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.NotFound && message.IndexOf("notFound", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}