using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace MobileAdmin.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class ComputerController : Controller
    {
        //
        // GET: /Computer/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Name(string computerName)
        {
            ViewBag.ComputerName = computerName;
            return View();
        }

        #region EnableAD
        public ActionResult EnableAD(string computerName)
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            proc.StartInfo.Password = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", "/c enable.cmd " + computerName);
            proc.StartInfo.WorkingDirectory = @"C:\Manage\EnableAD";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            ViewBag.ComputerName = computerName;
            return View();
        }

        public ActionResult MoveInAD(string computerName, string ADPath)
        {
            const int ldapErrorInvalidCredentials = 0x31;
            ViewBag.ComputerName = computerName;
            ViewBag.CurrentOUDN = ADPath;

            string server = Domain.GetDomain() + ":636";
            string domain = Domain.GetDomain();

            SecureString secure = AdminAccounts.GetAdminAccount3().SecurePassword;

            if (String.IsNullOrWhiteSpace(ADPath))
            {
                ADPath = ADPaths.GetDefaultPath();
            }

            try
            {
                using (var ldapConnection = new LdapConnection(server))
                {
                    var networkCredential = new NetworkCredential(AdminAccounts.GetAdminAccount3().UserName, secure, Domain.GetDomain());
                    ldapConnection.SessionOptions.SecureSocketLayer = true;
                    ldapConnection.AuthType = AuthType.Negotiate;
                    ldapConnection.Bind(networkCredential);

                    string DistinguishedName = GetObjectDistinguishedName(objectClass.computer, returnType.distinguishedName, computerName, Domain.GetDomain());
                    List<string> DNList = DistinguishedName.Split(',').ToList();
                    string finalOU = DNList.Skip(1).Take(1).First();
                    string finalOUName = finalOU.Substring(3, finalOU.Length - 3);

                    ViewBag.CurrentOU = finalOUName;

                    List<string> NewDNList = ADPath.Split(',').ToList();
                    string parentOU = NewDNList.Skip(1).Take(1).First();
                    string parentOUName = parentOU.Substring(3, parentOU.Length - 3);

                    string parentOUDN = string.Join(",", NewDNList.Skip(1).ToArray());

                    ViewBag.ParentOUDN = parentOUDN;

                    List<string> Children = EnumerateOU(ADPath);
                    ViewBag.Children = Children;


                }
            }
            catch (LdapException ldapException)
            {
                if (ldapException.ErrorCode.Equals(ldapErrorInvalidCredentials))
                {
                }
            }

            return View();
        }

        public ActionResult MoveInADConfirmed(string computerName, string ADPath)
        {
            
            ViewBag.ComputerName = computerName;
            
            string DistinguishedName = GetObjectDistinguishedName(objectClass.computer, returnType.distinguishedName, computerName, Domain.GetDomain());

            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", "/c move.cmd \"" + DistinguishedName.Replace("LDAP://","") + "\" \"" + ADPath + "\"");
            proc.StartInfo.WorkingDirectory = @"C:\Manage\MoveAD";
            proc.StartInfo.UseShellExecute = false;
            
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            return View();
        }

        public enum objectClass
        {
            user, group, computer
        }
        public enum returnType
        {
            distinguishedName, ObjectGUID
        }

        public static string GetObjectDistinguishedName(objectClass objectCls,
    returnType returnValue, string objectName, string LdapDomain)
        {
            string distinguishedName = string.Empty;
            string connectionPrefix = "LDAP://" + LdapDomain;
            DirectoryEntry entry = new DirectoryEntry(connectionPrefix);
            DirectorySearcher mySearcher = new DirectorySearcher(entry);

            switch (objectCls)
            {
                case objectClass.user:
                    mySearcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                    break;
                case objectClass.group:
                    mySearcher.Filter = "(&(objectClass=group)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                    break;
                case objectClass.computer:
                    mySearcher.Filter = "(&(objectClass=computer)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                    break;
            }
            SearchResult result = mySearcher.FindOne();

            if (result == null)
            {
                throw new NullReferenceException
                ("unable to locate the distinguishedName for the object " +
                objectName + " in the " + LdapDomain + " domain");
            }
            DirectoryEntry directoryObject = result.GetDirectoryEntry();
            if (returnValue.Equals(returnType.distinguishedName))
            {
                distinguishedName = "LDAP://" + directoryObject.Properties
                    ["distinguishedName"].Value;
            }
            if (returnValue.Equals(returnType.ObjectGUID))
            {
                distinguishedName = directoryObject.Guid.ToString();
            }
            entry.Close();
            entry.Dispose();
            mySearcher.Dispose();
            return distinguishedName;
        }

        public List<string> EnumerateOU(string OuDn)
        {
            List<string> alObjects = new List<string>();
            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + OuDn);
                foreach (DirectoryEntry child in directoryObject.Children)
                {
                    string childPath = child.Path.ToString();
                    childPath = childPath.Remove(0, 7);
                    if (childPath.StartsWith("OU"))
                    {
                        alObjects.Add(childPath);
                    }
                    //remove the LDAP prefix from the path

                    child.Close();
                    child.Dispose();
                }
                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message.ToString());
            }
            return alObjects;
        }

        public void MoveAD(string objectLocation, string newLocation)
        {
            DirectoryEntry eLocation = new DirectoryEntry("LDAP://" + objectLocation);
            DirectoryEntry nLocation = new DirectoryEntry("LDAP://" + newLocation);
            string newName = eLocation.Name;
            eLocation.MoveTo(nLocation, newName);
            nLocation.Close();
            eLocation.Close();
        }
        #endregion


        #region USMT

        [HttpGet]
        public ActionResult USMT(string computerName)
        {
            ViewBag.ComputerName = computerName;

            return View();
        }

        [HttpPost]
        public ActionResult StartUSMT(string computerName, string user, string targetIP)
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c schedUSMT.cmd {0} {1} {2}", computerName, user, targetIP));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\USMT";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            ViewBag.ComputerName = computerName;
            return View();
        }

        #endregion

        #region TSM

        [HttpGet]
        public ActionResult TSM(string computerName)
        {
            ViewBag.ComputerName = computerName;

            return View();
        }

        [HttpPost]
        public ActionResult StartTSM(string computerName,string user)
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c startTSM.cmd {0} {1}", computerName, user));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\TSM";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            ViewBag.ComputerName = computerName;
            return View();
        }

        #endregion

        #region MapDrive
        [HttpGet]
        public ActionResult MapDrive(string computerName)
        {
            ViewBag.ComputerName = computerName;

            return View();
        }

        [HttpPost]
        public ActionResult MapDrive(string computerName, string driveLetter, string networkDrive, string userName)
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount1().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount1().SecurePassword;

            string DriveLetter = FormatDriveLetter(driveLetter);
            string UserName = FormatUserName(userName);

            proc.StartInfo.Domain = Domain.GetDomain();
            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", String.Format(@"/c mapDrive.cmd {0} {1} {2} {3}", computerName, DriveLetter, networkDrive, UserName));
            proc.StartInfo.WorkingDirectory = @"C:\Manage\MapDrive";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            ViewBag.ComputerName = computerName;
            return View();
        }

        private string FormatUserName(string userName)
        {
            if (userName.ToUpper().StartsWith(Domain.AddDomainToUsername("").ToUpper()))
            {
                return userName;
            }
            else
            {
                return Domain.AddDomainToUsername(userName);
            }
        }

        private string FormatDriveLetter(string driveLetter)
        {
            if (!driveLetter.EndsWith(":"))
            {
                return driveLetter + ":";
            }

            return driveLetter;
        }

        #endregion
    }
}
