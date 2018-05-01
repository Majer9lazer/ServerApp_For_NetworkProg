using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Json;
using ServerApplication_For_NetworkProg.UserData.GroupInfo;
using ServerApplication_For_NetworkProg.UserData.StudentInfo;
using ServerApplication_For_NetworkProg.UserData.TeacherInfo;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace ServerApplication_For_NetworkProg.Modules
{
    public sealed class TestModule : NancyModule
    {
        public TestModule()
        {
            Get("/GetCurrentDirectory", a => AppContext.BaseDirectory);
            Get("/GetUserDataDb", a =>
            {
                DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent;
                return dir?.FullName;
            });
            Get("/GetPasswordFromRabbitMq", a => "liza1999");
            Get("/GetStudents", arg => Response.AsJson(Student.GetStudents()));
            Get("/GetGroups", arg => Response.AsJson(Group.GetGroups()));
            Get("/GetTeachers", a => Response.AsJson(Teacher.GetTeachers()));
            Get("/GetServerIp", a =>
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "Ip - wasnt found";
            });
            Post("/AddStudent/{StudentName}", a =>
            {
                Student.Add(new Student(a.StudentName));
                return $"Student {a.StudenName} was added to dataBase";
            });
            Post("/UploadFile/{FileExtension}/{GroupName}/{TeacherName}",async (parameters) =>
           {
               DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent;
               Directory.CreateDirectory(dir.FullName + $"/{parameters.TeacherName}");
               DirectoryInfo Info = Directory.
                    CreateDirectory(dir.FullName + $"/{parameters.TeacherName}/{parameters.GroupName}");
               var file = Request.Files.FirstOrDefault();
               using (Stream output = File.OpenWrite
                   (Info.FullName + $"/{parameters.GroupName}{parameters.FileExtension}"))
               using (Stream input = file.Value)
               {
                   input.CopyTo(output);
               }
               return HttpStatusCode.OK.ToString();
           });
            
        }
    }
}
