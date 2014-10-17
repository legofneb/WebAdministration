using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Web;
using MobileAdmin.Models;

namespace MobileAdmin
{
    public enum USMTMethod
    {
        Full, Attach
    }

    public class USMT : Program
    {
        public Computer _computer;

        public USMT(Computer comp, USMTMethod method = USMTMethod.Full)
        {
            _computer = comp;

            Logger usmtLog = new Logger(GlobalHost.ConnectionManager.GetHubContext<USMTHub>());
            _log = usmtLog;

            USMTStep1 step1 = new USMTStep1(comp, usmtLog);
            USMTStep2 step2 = new USMTStep2(comp, usmtLog);

            if (method == USMTMethod.Full)
            {
                _programSteps.Add(step1);
                _programSteps.Add(step2);
            }
            else if (method == USMTMethod.Attach)
            {
                _programSteps.Add(step2); // If we are attaching, only use the second step
            }
        }

        public override void Run()
        {
            foreach (var step in _programSteps)
            {
                step.Execute();
            }

            USMTList list = USMTList.Instance();
            list.RemoveProgram(_computer.IPAddress);
        }

        public string GetStatus()
        {
            return _log.GetStatus();
        }
    }

    public class USMTStep2 : ProgramStep
    {
        private Computer _comp;

        public USMTStep2(Computer comp, Logger log) : base(log)
        {
            _comp = comp;
        }

        public override bool Execute()
        {
            _log.UpdateStatus("Checking that USMT has started", _comp.IPAddress);
            if (PreConditionCheck())
            {
                _log.UpdateStatus("USMT has finished, sending Load Command", _comp.IPAddress);
                Run();
            }

            _log.UpdateStatus("Load Command has been sent, Monitoring has completed.", _comp.IPAddress);
            return true;
        }

        protected override bool PreConditionCheck()
        {
            int errorCount = 0;
            bool error = false;
            long filesize = 0;
            bool writeOnce = false;

            while (!error)
            {
                if (File.Exists(@"\\" + _comp.NewIPAddress + @"\C$\restore\printers.txt"))
                {
                    // USMT is done...
                    return true;
                }
                else
                {
                    // check to make sure USMT file exists

                    if (File.Exists(@"\\" + _comp.NewIPAddress + @"\C$\restore\USMT\USMT.MIG"))
                    {
                        // GOLDEN!
                        if (!writeOnce)
                        {
                            _log.Write("USMT file has been created on new machine", _comp.IPAddress);
                            writeOnce = true;
                            errorCount = 0;
                        }

                        // check to make sure USMT file is increasing

                        FileInfo fileInfo = new FileInfo(@"\\" + _comp.NewIPAddress + @"\C$\restore\USMT\USMT.MIG");
                        if (fileInfo.Length == filesize)
                        {
                            // fileSize hasn't changed
                            errorCount++;
                        }
                        else
                        {
                            // fileSize has changed...Hooray!
                            filesize = fileInfo.Length;
                            _log.UpdateStatus("USMT File Transfer Status: " + ByteToString(filesize), _comp.IPAddress);
                            errorCount = 0;
                        }

                    }
                    else
                    {
                        errorCount++;
                    }
                }

                Thread.Sleep(10000);

                if (errorCount >= 100)
                {
                    // soo many errors......... don't proceed
                    error = true;
                    _log.Write("Something went wrong...USMT halted", _comp.IPAddress);
                }
            }

            return false;
        }

        private string ByteToString(long p)
        {
            string ByteString = p.ToString();

            if (ByteString.Length > 9)
            {
                return ByteString.Substring(0, ByteString.Length - 9) + "." + ByteString.Substring(ByteString.Length - 9, 3) + " GB";
            }
            else if (ByteString.Length > 6)
            {
                return ByteString.Substring(0, ByteString.Length - 6) + " MB";
            }
            else if (ByteString.Length > 3)
            {
                return ByteString.Substring(0, ByteString.Length - 3) + " KB";
            }
            

            return p.ToString() + " B";

        }

        protected override void Run()
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            proc.StartInfo.Password = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain(); ;

            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c loadUSMT.cmd {0}", _comp.NewIPAddress));
            
            proc.StartInfo.WorkingDirectory = @"C:\Manage\USMT";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += (sender, args) => UpdateStatus(args.Data);
            proc.ErrorDataReceived += (sender, args) => UpdateStatus(args.Data);

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit(60);
        }

        private void UpdateStatus(string output)
        {
            _log.Write(output, _comp.IPAddress);
        }

        protected override bool PostConditionCheck()
        {
            return true;
        }
    }

    public class USMTStep1 : ProgramStep
    {
        private int noErrorCount;
        private Computer _comp;

        public USMTStep1(Computer comp, Logger log) : base(log)
        {
            _comp = comp;
        }

        public override bool Execute()
        {
            _log.UpdateStatus("Starting Transfer.", _comp.IPAddress);
            Run();
            _log.UpdateStatus("Sent Execution Command.", _comp.IPAddress);
            return true;
        }

        protected override bool PreConditionCheck()
        {
            return true;
        }

        protected override void Run()
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            proc.StartInfo.Password = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            if (_comp.USMTGID != "aulease")
            {
                proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c schedUSMT.cmd {0} {1} {2}", _comp.IPAddress, _comp.USMTGID, _comp.NewIPAddress));
            }
            else
            {
                proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c schedUSMTasAULease.cmd {0} {1}", _comp.IPAddress, _comp.NewIPAddress));
            }
            proc.StartInfo.WorkingDirectory = @"C:\Manage\USMT";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.EnableRaisingEvents = true;
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
                UpdateStatus("Done??");
            }
            else if (output == null)
            {
                noErrorCount++;
            }
            else
            {
                _log.Write(output, _comp.IPAddress);
            }
        }

        protected override bool PostConditionCheck()
        {
            return true;
        }

    }
}