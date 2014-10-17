using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace MobileAdmin
{
    public class ADPaths
    {
        public static string GetDefaultPath()
        {
            string defaultPath = WebConfigurationManager.AppSettings["defaultADPath"];
            return defaultPath;
        }
    }
}