using Google.Apis.Classroom.v1.Data;
using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class CourseStudents
    {
        public HashSet<string> Students { get; set; }

        public int Count
        {
            get
            {
                return this.Students.Count;
            }
        }

        public CourseStudents()
        {
            this.Students = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public CourseStudents(HashSet<string> students)
        {
            this.Students = students;
        }

        public HashSet<string> GetAllStudents()
        {
            return this.Students;
        }

        public List<Student> ToStudentList()
        {
            List<Student> members = new List<Student>();

            foreach (string address in this.Students)
            {
                Student student = new Student
                {
                    UserId = address
                };

                members.Add(student);
            }

            return members;
        }

        public void AddStudent(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.UserId))
            {
                return;
            }

            this.AddStudent(student.UserId);
        }

        public void AddStudent(string address)
        {
            this.Students.Add(address);
        }

        public void RemoveStudent(string student)
        {
            this.Students.Remove(student);
        }

        public void RemoveStudents(IEnumerable<string> students)
        {
            foreach (string student in students)
            {
                this.RemoveStudent(student);
            }
        }

        public void MergeStudents(CourseStudents s)
        {
            foreach (string item in s.Students)
            {
                this.Students.Add(item);
            }
        }
    }
}