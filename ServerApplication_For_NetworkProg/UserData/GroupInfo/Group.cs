using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ServerApplication_For_NetworkProg.RabbitMq;
using ServerApplication_For_NetworkProg.UserData.StudentInfo;

namespace ServerApplication_For_NetworkProg.UserData.GroupInfo
{

    [Serializable]
    public class Group
    {
        private readonly PublishMessageClass _publish = new PublishMessageClass();
        public Group()
        {
            _groupDb.Changed += _groupDb_Changed;
        }

        private void _groupDb_Changed(object sender, XObjectChangeEventArgs e)
        {
            XElement s = (XElement)sender;
          
            Console.WriteLine($"{e.ObjectChange == XObjectChange.Value} object = {sender} , type = {sender.GetType()}");

            if (e.ObjectChange == XObjectChange.Add)
                _publish.PublishMessage
                    (new Group()
                    {
                        GroupName = s.Name == nameof(GroupName) ? s.Value : null,
                        Students = s.Element("students").Elements()
                            .Select(ss => new Student() { StudentName = ss.Value }).ToList()

                    }, queueName: "StudentAdded");
        }

        private enum GroupNames
        {
            SDP, SEP, PMP, PUB, SMB
        }

        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent?.Parent;
        private static readonly FileInfo F1 = new FileInfo(PathToDataBase.FullName + @"/UserData/GroupInfo/Group_Db.xml");
        private static readonly XDocument _groupDb = XDocument.Load(F1.FullName);
        private readonly Random _rnd = new Random();

        public string GroupName { get; set; }
        public int? CountOfPupils { get; set; }
        public List<Student> Students = new List<Student>();
        public bool AddGroupWithSudents()
        {
            if (Students.Count <= 0 || _groupDb.Root == null) return false;
            if (!IsGroupIsExists(GroupName))
            {
                _groupDb.Root.Add(new XElement("group", new XElement(nameof(GroupName), GroupName)));
                _groupDb.Save(F1.FullName);
                _groupDb.Root?.Elements().Elements().FirstOrDefault(w => w.Value == GroupName)?.Parent?.Add(new XElement("students"));
                _groupDb.Save(F1.FullName);
                XElement studentElement = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Value == GroupName)?.Parent?.Element("students");

                foreach (Student s in Students)
                    studentElement?.Add(new XElement(nameof(s.StudentName), s.StudentName));

                _groupDb.Save(F1.FullName);
            }
            else
            {
                List<string> group = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == nameof(GroupName) && f.Value == GroupName)?.Parent?.Element("students")?.Elements().Select(s => s.Value).ToList();

                if (@group != null)
                {
                    List<string> both = Students.Select(s => s.StudentName).Intersect(@group).ToList();

                    if (both.Count == 0)
                    {
                        XElement studentElement = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == "students");
                        foreach (Student s in Students)
                            studentElement?.Add(new XElement(nameof(s.StudentName), s.StudentName));

                        _groupDb.Save(F1.FullName);
                    }
                    else
                    {
                        foreach (string s in both)
                            if (Students.Exists(e => e.StudentName == s))
                                Students.Remove(Students.ElementAt(Students.IndexOf(Students.FirstOrDefault(f => f.StudentName == s))));

                        return AddGroupWithSudents();
                    }
                }
            }
            return true;
        }
        private bool IsGroupIsExists(string groupName)
        {
            XElement group = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == nameof(GroupName) && f.Value == GroupName);
            if (group == null)
                return false;
            return true;
        }

        private List<string> GetPossibleCountOfGroups()
        {
            int currentYear = (DateTime.Now.Year - 1) % 100;
            List<string> groupsList = new List<string>();
            for (int m = 0; m < 2; m++)
            {
                for (int l = 0; l <= (int)GroupNames.SMB; l++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        for (int i = 0; i < currentYear; i++)
                        {
                            int groupNum = _rnd.Next(1, currentYear);
                            string groupName = ((GroupNames)_rnd.Next(0, (int)GroupNames.SMB)).ToString();
                            if (groupNum >= 10)
                            {
                                groupsList.Add(groupName + $"-{groupNum}{_rnd.Next(1, 4)}");
                            }
                            else
                            {
                                groupsList.Add(groupName + $"-0{groupNum}{_rnd.Next(1, 4)}");
                            }
                        }
                    }
                }
            }

            return groupsList;
        }

        public static void RemoveEmptyFields()
        {
            _groupDb.Root?.Elements().Elements().Elements().Where(w => string.IsNullOrEmpty(w.Value)).Remove();
            _groupDb.Root?.Elements().Elements().Where(w => string.IsNullOrEmpty(w.Value)).Remove();
            Console.WriteLine($"Empty Elements were removed , count of empty elements - " +
                              $"{(_groupDb.Root?.Elements().Elements().Elements())?.Count(f => string.IsNullOrEmpty(f.Value))}");
            _groupDb.Save(F1.FullName);
        }
        public void GenerateData()
        {
            List<Student> students = Student.GetStudents();
            foreach (string @group in GetPossibleCountOfGroups())
            {
                Group g = new Group
                {
                    GroupName = @group
                };
                int randStudentCountIngroup = _rnd.Next(5, 16);
                for (int i = 0; i < randStudentCountIngroup; i++)
                {
                    g.Students.Add(students.ElementAt(_rnd.Next(0, students.Count)));
                }
                g.AddGroupWithSudents();
            }

            Console.WriteLine("Done");
        }
        public static List<Group> GetCurrentGroupByName(string groupName) => !string.IsNullOrEmpty(groupName)
            ? _groupDb.Root?.Elements().Elements()
                .Where(f => f.Name == nameof(GroupName) && f.Value.Contains(groupName))
                .Select(s => new Group { GroupName = s.Value }).ToList()
            : null;

        public static List<Group> GetGroups() =>
            _groupDb.Root?.Elements().Select(s => new Group()
            {
                GroupName = s.Element("GroupName")?.Value,
                Students = s.Element("students")?.Elements().Select(ss => new Student()
                {
                    StudentName = ss.Value
                }).ToList()
            }).ToList();

        public static List<Student> GetListOfStudentsByGroupName(string groupName, string studentName)
        {
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(studentName))
                return _groupDb.Root?.Elements().Elements()
                    .Where(w => w.Name == nameof(GroupName) && w.Value.Contains(groupName))
                    .Select(s => new Group
                    {
                        Students = s.Parent?.Element("students")?.Elements()
                            .Where(w => w.Value.Contains(studentName))
                            .Select(s1 => new Student { StudentName = s1.Value })
                            .ToList()
                    }).Select(s => s.Students).ToList()[0].ToList();

            return null;
        }

        public static List<Student> GetAllStudentsByGroupName(string groupName) => string.IsNullOrEmpty(groupName) ? null :
            _groupDb.Root?.Elements().Elements()
                .FirstOrDefault(w => w.Name == nameof(GroupName) && w.Value == (groupName))?
                .Parent?
                .Element("students")?
                .Elements().Select(s => new Student()
                {
                    StudentName = s.Value
                }).ToList();


    }
}
