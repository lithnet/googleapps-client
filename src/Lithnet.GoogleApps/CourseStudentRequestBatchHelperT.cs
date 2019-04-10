using Google.Apis.Requests;
using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps
{
    internal class CourseStudentRequestBatchHelper<T>
    {
        public bool IgnoreExistingStudent { get; set; }
        public bool IgnoreMissingStudent { get; set; }
        public int BaseCount { get; set; }
        public Dictionary<string, ClientServiceRequest<T>> RequestsToRetry { get; set; }
        public ClientServiceRequest<T> Request { get; set; }
        public List<string> FailedStudents { get; set; }
        public List<Exception> Failures { get; set; }
    }
}