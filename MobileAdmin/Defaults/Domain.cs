using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace MobileAdmin
{
    public class Domain
    {
        public static string GetDomain()
        {
            return WebConfigurationManager.AppSettings["domain"];
        }

        public static string AddDomainToUsername(string username)
        {
            string fullDomain = WebConfigurationManager.AppSettings["domain"];
            string ShortDomainQualifier = fullDomain.Substring(0, fullDomain.IndexOf("."));

            return ShortDomainQualifier + @"\" + username;
        }
    }
}