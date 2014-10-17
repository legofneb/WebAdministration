using Microsoft.AspNet.SignalR;
using Microsoft.Win32;
using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Web.Http;

namespace MobileAdmin.API
{
    [System.Web.Http.Authorize]
    [InitializeSimpleMembership]
    public class WordPerfectController : ApiController
    {
        Computer comp;
        int noErrorCount;

        // GET api/wordperfect
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/wordperfect/5
        public bool Get(string id)
        {
            return IsAppInstalled(id);
        }

        // POST api/wordperfect
        public void Post([FromBody]Computer value)
        {
            comp = value;

            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            proc.StartInfo.Password = AdminAccounts.GetAdminAccount1().SecurePassword;
            proc.StartInfo.Domain = Domain.GetDomain();

            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c installWordPerfect.cmd {0}", value.IPAddress));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\WordPerfect";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            //proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += (sender, args) => UpdateStatus(args.Data);
            proc.ErrorDataReceived += (sender, args) => UpdateStatus(args.Data);

            noErrorCount = 0;

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit(60);

        }

        private void UpdateStatus(string output)
        {
            if (output == null && noErrorCount > 0)
            {
                UpdateStatus("Computer Running Script...");
                UpdateStatus("Waiting 5 seconds to check status...");
                Thread.Sleep(5000);
                UpdateStatus("Performing Post-Install Check");
                CheckForProgram();
            }
            else if (output == null)
            {
                noErrorCount++;
            }
            else
            {
                IHubContext tsmHub = GlobalHost.ConnectionManager.GetHubContext<WordPerfectHub>();
                tsmHub.Clients.All.updateStatus(output);
            }
        }

        private void CheckForProgram()
        {
            bool programInstalled = false;

            for (int i = 0; i < 6 && !programInstalled; i++)
            {
                programInstalled = IsAppInstalled(comp.IPAddress);

                if (!programInstalled)
                {
                    UpdateStatus("Word Perfect not found, trying again in 5 minutes.");
                    Thread.Sleep(300000);
                }
                else
                {
                    UpdateStatus("Word Perfect has bene installed!!");
                    return;
                }
            }

            UpdateStatus("Install Timed out...try manually checking...");
        }

        public static bool IsAppInstalled(string p_machineName)
        {
            ConnectionOptions op = new ConnectionOptions();
            op.Username = AdminAccounts.GetAdminAccount2().UserName;
            op.Password = AdminAccounts.GetAdminAccount2().Password;
            ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\root\cimv2", p_machineName), op);
            scope.Connect();
            ManagementPath path = new ManagementPath("Win32_Product");
            ManagementClass programs = new ManagementClass(scope, path, null);

            foreach (ManagementObject program in programs.GetInstances())
            {
                object programName = program.GetPropertyValue("Name");

                if (programName != null && programName.ToString().Equals("WordPerfect Office 11", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // PUT api/wordperfect/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/wordperfect/5
        public void Delete(int id)
        {
        }
    }
}
