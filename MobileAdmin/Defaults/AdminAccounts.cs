using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MobileAdmin.Models;
using System.Web.Configuration;
using System.Security;

namespace MobileAdmin
{
    public class AdminAccounts
    {
        public static UserModel GetAdminAccount1()
        {
            UserModel admin = new UserModel();
            admin.UserName = WebConfigurationManager.AppSettings["user1"];
            admin.Password = WebConfigurationManager.AppSettings["password1"];
            SecureString securePass = new SecureString();
            foreach (char c in admin.Password)
            {
                securePass.AppendChar(c);
            }
            admin.SecurePassword = securePass; 

            return admin;
        }

        public static UserModel GetAdminAccount2()
        {
            UserModel admin = new UserModel();
            admin.UserName = WebConfigurationManager.AppSettings["user2"];
            admin.Password = WebConfigurationManager.AppSettings["password2"];
            SecureString securePass = new SecureString();
            foreach (char c in admin.Password)
            {
                securePass.AppendChar(c);
            }
            admin.SecurePassword = securePass;

            return admin;
        }

        public static UserModel GetAdminAccount3()
        {
            UserModel admin = new UserModel();
            admin.UserName = WebConfigurationManager.AppSettings["user3"];
            admin.Password = WebConfigurationManager.AppSettings["password3"];
            SecureString securePass = new SecureString();
            foreach (char c in admin.Password)
            {
                securePass.AppendChar(c);
            }
            admin.SecurePassword = securePass;

            return admin;
        }

        public static UserModel GetAdminAccount4()
        {
            UserModel admin = new UserModel();
            admin.UserName = WebConfigurationManager.AppSettings["user4"];
            admin.Password = WebConfigurationManager.AppSettings["password4"];
            SecureString securePass = new SecureString();
            foreach (char c in admin.Password)
            {
                securePass.AppendChar(c);
            }
            admin.SecurePassword = securePass;

            return admin;
        }
    }
}