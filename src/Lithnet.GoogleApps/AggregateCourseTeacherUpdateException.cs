using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps
{
    public class AggregateCourseTeacherException : Exception
    {
        public IEnumerable<string> FailedTeachers;

        public IList<Exception> Exceptions;

        public AggregateCourseTeacherException()
            : base()
        {
        }

        public AggregateCourseTeacherException(string courseId, IEnumerable<string> teacherKeys)
            : base($"The course teacher update operation failed. Course ID {courseId}")
        {
            this.FailedTeachers = teacherKeys;
        }

        public AggregateCourseTeacherException(string courseId, IEnumerable<string> teacherKeys, IList<Exception> exceptions)
            : this(courseId, teacherKeys)
        {
            this.Exceptions = exceptions;
        }
    }
}
