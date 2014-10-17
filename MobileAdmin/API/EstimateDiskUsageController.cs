using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;

namespace MobileAdmin.API
{
    public class DiskUsage
    {
        public string TotalSpace { get; set; }
        public string FreeSpace { get; set; }
        public string UsedSpace { get; set; }
        public string PercentUsed { get; set; }
        public string PercentFree { get; set; }
    }

    [Authorize]
    [InitializeSimpleMembership]
    public class EstimateDiskUsageController : ApiController
    {
        // GET api/estimatediskusage
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/estimatediskusage/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/estimatediskusage
        public DiskUsage Post([FromBody]Computer computer)
        {
            string IPAddress = computer.IPAddress;

            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c getDiskUsage.bat {0} C:", IPAddress));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\EstimateDiskSpace";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit(10);

            string[] lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            DiskUsage diskUsage = new DiskUsage()
            {
                TotalSpace = lines[0].Substring(13, lines[0].Length - 13),
                FreeSpace = lines[1].Substring(12, lines[1].Length - 12),
                UsedSpace = lines[2].Substring(12, lines[2].Length - 12),
                PercentUsed = lines[3].Substring(14, lines[3].Length - 14),
                PercentFree = lines[4].Substring(14, lines[4].Length - 14)
            };

            return diskUsage;
        }

        // PUT api/estimatediskusage/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/estimatediskusage/5
        public void Delete(int id)
        {
        }
    }
}
