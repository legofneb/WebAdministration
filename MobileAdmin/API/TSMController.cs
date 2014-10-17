using Microsoft.AspNet.SignalR;
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
    public class TSMController : ApiController
    {
        int noErrorCount;
        Computer comp;

        // GET api/tsm
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/tsm/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/tsm
        public void Post([FromBody]Computer value)
        {
            comp = value;

            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            proc.StartInfo.Password = AdminAccounts.GetAdminAccount1().SecurePassword;
            proc.StartInfo.Domain = Domain.GetDomain();

            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c startTSM.cmd {0} {1}", value.IPAddress, value.TSMNodeName));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\TSM";
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
                CheckServices();
            }
            else if (output == null)
            {
                noErrorCount++;
            }
            else
            {
                IHubContext tsmHub = GlobalHost.ConnectionManager.GetHubContext<TSMHub>();
                tsmHub.Clients.All.updateStatus(output);
            }
        }

        private void CheckServices()
        {
            ConnectionOptions op = new ConnectionOptions();
            op.Username = AdminAccounts.GetAdminAccount2().UserName;
            op.Password = AdminAccounts.GetAdminAccount2().Password;
            ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\root\cimv2", comp.IPAddress), op);
            scope.Connect();
            ManagementPath path = new ManagementPath("Win32_Service");
            ManagementClass services = new ManagementClass(scope, path, null);

            bool foundTSM = false;

            for (int i = 0; i < 5 && !foundTSM; i++ )
            {
                foundTSM = CheckForTSMService(services);

                if (!foundTSM)
                {
                    UpdateStatus("Service error, trying again.");
                    Thread.Sleep(1000);
                }
            }

            if (!foundTSM)
            {
                UpdateStatus("There was an error setting up TSM Scheduler.");
            }
        }

        private bool CheckForTSMService(ManagementClass services)
        {
            bool foundTSM = false;

            foreach (ManagementObject service in services.GetInstances())
            {
                if (service.GetPropertyValue("DisplayName").Equals("tsm_scheduler"))
                {
                    if (service.GetPropertyValue("State").ToString().ToLower().Equals("running"))
                    {
                        foundTSM = true;
                        UpdateStatus("TSM is running for " + comp.LeaseTag);
                    }
                    else
                    {
                        UpdateStatus("TSM is not running");
                    }
                }
            }

            return foundTSM;
        }

        // PUT api/tsm/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/tsm/5
        public void Delete(int id)
        {
        }
    }
}
