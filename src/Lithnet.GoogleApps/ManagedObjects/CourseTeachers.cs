using Google.Apis.Classroom.v1.Data;
using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class CourseTeachers
    {

        public HashSet<string> Teachers { get; set; }

        public int Count
        {
            get
            {
                return this.Teachers.Count;
            }
        }

        public CourseTeachers()
        {
            this.Teachers = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public CourseTeachers(HashSet<string> teachers)
        {
            this.Teachers = teachers;
        }


        public HashSet<string> GetAllTeachers()
        {
            return this.Teachers;
        }

        public List<Teacher> ToTeacherList()
        {
            List<Teacher> members = new List<Teacher>();

            foreach (string address in this.Teachers)
            {
                Teacher teacher = new Teacher
                {
                    UserId = address

                };

                members.Add(teacher);
            }

            return members;
        }


        public void AddTeacher(Teacher teacher)
        {
            if (string.IsNullOrWhiteSpace(teacher.UserId))
            {
                return;
            }

            this.AddTeacher(teacher.UserId);

        }

        public void AddTeacher(string address)
        {
            this.Teachers.Add(address);
        }

        public void RemoveTeacher(string teacher)
        {
            this.Teachers.Remove(teacher);
        }

        public void RemoveTeachers(IEnumerable<string> teachers)
        {
            foreach (string teacher in teachers)
            {
                this.RemoveTeacher(teacher);
            }
        }

        public void MergeTeachers(CourseTeachers ts)
        {
            foreach (string item in ts.Teachers)
            {
                this.Teachers.Add(item);
            }

        }
    }
}