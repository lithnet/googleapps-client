using Google.Apis.Requests;
using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps
{
    internal class CourseTeacherRequestBatchHelper<T>
    {
        public bool IgnoreExistingTeacher { get; set; }
        public bool IgnoreMissingTeacher { get; set; }
        public int BaseCount { get; set; }
        public Dictionary<string, ClientServiceRequest<T>> RequestsToRetry { get; set; }
        public ClientServiceRequest<T> Request { get; set; }
        public List<string> FailedTeachers { get; set; }
        public List<Exception> Failures { get; set; }

    }
}