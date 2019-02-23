using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps
{
    public class AggregateCourseStudentException : Exception
    {
        public IEnumerable<string> FailedStudents;

        public IList<Exception> Exceptions;

        public AggregateCourseStudentException()
            : base()
        {
        }

        public AggregateCourseStudentException(string courseId, IEnumerable<string> studentKeys)
            : base($"The course student update operation failed. Course ID {courseId}")
        {
            this.FailedStudents = studentKeys;
        }

        public AggregateCourseStudentException(string courseId, IEnumerable<string> studentKeys, IList<Exception> exceptions)
            : this(courseId, studentKeys)
        {
            this.Exceptions = exceptions;
        }
    }
}
