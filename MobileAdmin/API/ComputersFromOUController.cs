using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;

namespace MobileAdmin.API
{
    [Authorize]
    [InitializeSimpleMembership]
    public class ComputersFromOUController : ApiController
    {
        // GET api/computerfromou
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/computerfromou/5
        public List<string> Get(string id)
        {
            List<string> ComputerNames = new List<string>();

            string ADPath = id;

            const int ldapErrorInvalidCredentials = 0x31;

            string server = Domain.GetDomain() + ":636";

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

                    foreach (var child in EnumerateOU(ADPath))
                    {
                        ComputerNames.Add(child.Split(',').ToList().Take(1).First().Substring(3, child.Split(',').ToList().Take(1).First().Length -3));
                    }


                }
            }
            catch (LdapException ldapException)
            {
                if (ldapException.ErrorCode.Equals(ldapErrorInvalidCredentials))
                {
                }
            }

            return ComputerNames;
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
                    if (childPath.StartsWith("CN"))
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

        // POST api/computerfromou
        public void Post([FromBody]string value)
        {
        }

        // PUT api/computerfromou/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/computerfromou/5
        public void Delete(int id)
        {
        }
    }
}
