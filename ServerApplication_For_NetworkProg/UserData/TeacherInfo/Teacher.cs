using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ServerApplication_For_NetworkProg.UserData.GroupInfo;
using ServerApplication_For_NetworkProg.UserData.StudentInfo;

namespace ServerApplication_For_NetworkProg.UserData.TeacherInfo
{
    [Serializable]
    public class Teacher
    {
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent?.Parent;
        private static readonly FileInfo F1 = new FileInfo(PathToDataBase.FullName + @"/UserData/TeacherInfo/Teachers_Db.xml");
        public static readonly XDocument TeacherDb = XDocument.Load(F1.FullName);

        public string TeacherName { get; set; }
        public string TeacherLastName { get; set; }
        public string TeacherGuid { get; set; }
        public string TeacherMail { get; set; }
        public List<Group> Groups { get; set; }


        public static List<Teacher> GetTeachers() => TeacherDb.Root?.Elements()
            .Select(s => new Teacher()
            {
                TeacherName = s.Element(nameof(TeacherName))?.Value,
                TeacherLastName = s.Element(nameof(TeacherLastName))?.Value,
                TeacherGuid = s.Element(nameof(TeacherGuid))?.Value,
                TeacherMail = s.Element(nameof(TeacherMail))?.Value,
                Groups = s.Element(nameof(Groups))?.Elements().Select(q => new Group
                {
                    GroupName = q.Value,
                    Students = Group.GetAllStudentsByGroupName(q.Value)
                }).ToList()
            }).ToList();
    }
}
