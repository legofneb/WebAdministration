using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileAdmin.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class NGController : Controller
    {
        //
        // GET: /NG/
        
        public ActionResult Index()
        {
            return PartialView();
        }

        public PartialViewResult Home()
        {
            return PartialView();
        }

        public PartialViewResult Actions()
        {
            return PartialView();
        }

        public PartialViewResult IP()
        {
            return PartialView();
        }

        public PartialViewResult EstimateDiskUsage()
        {
            return PartialView();
        }

        public PartialViewResult TSM()
        {
            return PartialView();
        }

        public PartialViewResult USMT()
        {
            return PartialView();
        }

        public PartialViewResult Admin()
        {
            return PartialView();
        }

        public PartialViewResult WordPerfect()
        {
            return PartialView();
        }

        public PartialViewResult MapDrive()
        {
            return PartialView();
        }

        public PartialViewResult IDF()
        {
            return PartialView();
        }

        public PartialViewResult USMTConsole()
        {
            return PartialView();
        }

        // TODO Add method to install Creative Cloud
    }
}
