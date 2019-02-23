using System;
using System.Collections.Generic;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Classroom.v1.Data;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    public class GoogleCourse
    {
        public GoogleCourse()
        {
            this.Errors = new List<Exception>();
            this.Course = new Course();
            this.Students = new CourseStudents();
            this.Teachers = new CourseTeachers();
        }

        public GoogleCourse(Course course)
            : this()
        {
            this.Course = course;
        }

        public Course Course { get; set; }

        public CourseStudents Students { get; set; }

        public CourseTeachers Teachers { get; set; }

        public List<Exception> Errors { get; private set; }

        internal bool IsComplete
        {
            get
            {
                lock (this)
                {
                    return this.LoadedStudents == this.RequiresStudents && this.LoadedTeachers == this.RequiresTeachers;
                }
            }
        }


        internal bool LoadedStudents { get; set; }

        internal bool LoadedTeachers { get; set; }

        internal bool RequiresStudents { get; set; }

        internal bool RequiresTeachers { get; set; }
    }
}
