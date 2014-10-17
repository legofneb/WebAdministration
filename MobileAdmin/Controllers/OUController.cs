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
    public class OUController : Controller
    {
        //
        // GET: /OU/

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Home()
        {
            return PartialView();
        }

        public PartialViewResult Actions()
        {
            return PartialView();
        }
    }
}
