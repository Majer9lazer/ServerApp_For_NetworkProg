using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using ServerApplication_For_NetworkProg.UserData.GroupInfo;
using ServerApplication_For_NetworkProg.UserData.StudentInfo;
using ServerApplication_For_NetworkProg.UserData.TeacherInfo;

namespace ServerApplication_For_NetworkProg
{
    public class Program
    {
        public static string Ip = "localhost", Login = "best", Password = "liza1999", VirtualHost = "/";

        public static void Main(string[] args)
        {
           
          
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build()
                .Run();
         
          
        }
    }
}
