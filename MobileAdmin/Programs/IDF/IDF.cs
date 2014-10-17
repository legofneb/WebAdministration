using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security;
using System.Threading;
using System.Web;

namespace MobileAdmin
{
    public class IDF : Program
    {
        public IDF(Computer comp) : base()
        {
            Logger idfLog = new Logger(GlobalHost.ConnectionManager.GetHubContext<IDFHub>());

            IDFStep1 step1 = new IDFStep1(comp, idfLog);

            _programSteps.Add(step1);
        }
    }

    public class IDFStep1 : ProgramStep
    {
        private int noErrorCount;
        private Computer _comp;

        public IDFStep1(Computer comp, Logger log)
            : base(log)
        {
            _comp = comp;
        }

        public override bool Execute()
        {
            if (!PreConditionCheck())
            {
                return false;
            }

            Run();
            return PostConditionCheck();
        }

        protected override bool PreConditionCheck()
        {
            if (IsAppInstalled(_comp.IPAddress))
            {
                _log.Write("IDF is already installed!", _comp.IPAddress);
                return false;
            }
            else
            {
                return true;
            };
        }

        protected override void Run()
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c installIDF.cmd {0}", _comp.IPAddress));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\IdentityFinder";
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
            _log.Write(output, _comp.IPAddress);
        }

        private bool CheckForProgram()
        {
            bool programInstalled = false;

            for (int i = 0; i < 6 && !programInstalled; i++)
            {
                programInstalled = IsAppInstalled(_comp.IPAddress);

                if (!programInstalled)
                {
                    _log.Write("IDF not found, trying again in 1 minute.", _comp.IPAddress);
                    Thread.Sleep(20000);
                }
                else
                {
                    _log.Write("IDF has been installed!!", _comp.IPAddress);
                    return true;
                }
            }

            _log.Write("Install Timed out...try manually checking...", _comp.IPAddress);
            return false;
        }

        public bool IsAppInstalled(string p_machineName)
        {
            ConnectionOptions op = new ConnectionOptions();
            op.Username = Domain.AddDomainToUsername(AdminAccounts.GetAdminAccount2().UserName);
            op.Password = AdminAccounts.GetAdminAccount2().Password;
            ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\root\cimv2", p_machineName), op);
            scope.Connect();
            ManagementPath path = new ManagementPath("Win32_Product");
            ManagementClass programs = new ManagementClass(scope, path, null);

            foreach (ManagementObject program in programs.GetInstances())
            {
                object programName = program.GetPropertyValue("Name");

                if (programName != null && programName.ToString().Equals("Identity Finder", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool PostConditionCheck()
        {
            _log.Write("Computer Running Script...", _comp.IPAddress);
            _log.Write("Waiting 5 seconds to check status...", _comp.IPAddress);
            Thread.Sleep(5000);
            _log.Write("Performing Post-Install Check", _comp.IPAddress);
            return CheckForProgram();
        }

    }
}