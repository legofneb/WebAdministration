using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;

namespace MobileAdmin.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return RedirectToAction("Index", "NG");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Kill()
        {
            return View();
        }

        public ActionResult KillAction()
        {
            Process proc = new Process();
            proc.StartInfo.UserName = AdminAccounts.GetAdminAccount4().UserName;
            SecureString secure = AdminAccounts.GetAdminAccount4().SecurePassword;

            proc.StartInfo.Password = secure;
            proc.StartInfo = new ProcessStartInfo(@"cmd.exe", "/c killAUComp.cmd");
            proc.StartInfo.WorkingDirectory = @"C:\kill\";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit(10);

            return RedirectToAction("Kill");
        }
    }
}
