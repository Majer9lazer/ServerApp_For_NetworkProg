using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using ServerApplication_For_NetworkProg.RabbitMq;

namespace ServerApplication_For_NetworkProg.UserData.StudentInfo
{
    [Serializable]
    public class Student
    {
        //Static read only peremennye
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppContext.BaseDirectory)).Parent?.Parent?.Parent;
        private static readonly FileInfo UserDataBaseFileInfo = new FileInfo(PathToDataBase.FullName + @"/UserData/StudentInfo/Students_Db.xml");
        public static readonly XDocument StudentDb = XDocument.Load(UserDataBaseFileInfo.FullName);
        private readonly PublishMessageClass _publish = new PublishMessageClass();
        public Student()
        {
            StudentDb.Changed += StudentDb_Changed;
        }
        public Student(string studentName)
        {
            StudentDb.Changed += StudentDb_Changed;
            StudentName = studentName;
        }
        private void StudentDb_Changed(object sender, XObjectChangeEventArgs e)
        {
            XElement s = (XElement)sender;
            Console.WriteLine(s.Value);
            Console.WriteLine($"{e.ObjectChange == XObjectChange.Value} object = {sender} , type = {sender.GetType()}");

            if (e.ObjectChange == XObjectChange.Add)
                _publish.PublishMessage
                    (new Student {StudentName = s.Value}, queueName: "StudentAdded");
        }

        public static void RemoveEmptyElements()
        {
            int? countofemptyvalues = (StudentDb.Root?.Elements())?.Count(w => string.IsNullOrEmpty(w.Value));
            while (countofemptyvalues>0)
            {
                StudentDb.Root?.Elements().Elements().Where(w => string.IsNullOrEmpty(w.Value)).Remove();
                StudentDb.Root?.Elements().Where(w => string.IsNullOrEmpty(w.Value)).Take(1000).Remove();
                StudentDb.Save(UserDataBaseFileInfo.FullName);
                Console.WriteLine($"Count of empty fields in students db = { (StudentDb.Root?.Elements())?.Count(w => string.IsNullOrEmpty(w.Value))}");
                countofemptyvalues -= 1000;
            }
            Console.WriteLine("Empty elements were removed successfully");
        }

        public string StudentName { get; set; }

        public static List<Student> GetStudents()
        {
            return StudentDb.Root?
                .Elements()
                .Elements()
                .Select(s =>
                    new Student { StudentName = s.Value })
                .ToList();
        }

        public static void Add(Student st)
        {
            StudentDb.Root?.Add(new XElement("student",
                new XElement(nameof(StudentName), st.StudentName)));
            StudentDb.Save(UserDataBaseFileInfo.FullName);
        }

        public static int Count()
        {
            if (StudentDb != null)
                return (int)StudentDb.Root?.Elements()?.Count();
            return 0;
        }

    }
}
