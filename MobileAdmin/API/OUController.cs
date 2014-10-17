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
    public class OUInfo
    {
        public List<OU> ListOfContainers { get; set; }
        public string CurrentContainer { get; set; }
    }

    public class OU
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
    }

    [Authorize]
    [InitializeSimpleMembership]
    public class OUController : ApiController
    {

        // GET api/ou
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/ou/5
        public OUInfo Get(string id)
        {
            List<OU> ListOfContainers = new List<OU>();

            string ADPath = id;

            const int ldapErrorInvalidCredentials = 0x31;

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

                    List<string> NewDNList = ADPath.Split(',').ToList();
                    string parentOU = NewDNList.Skip(1).Take(1).First();
                    string parentOUName = parentOU.Substring(3, parentOU.Length - 3);

                    string parentOUDN = string.Join(",", NewDNList.Skip(1).ToArray());

                    OU ParentOU = new OU() { ShortName = "..", FullName = parentOUDN };
                    ListOfContainers.Add(ParentOU);

                    foreach (var child in EnumerateOU(ADPath))
                    {
                        OU ChildOU = new OU()
                        {
                            ShortName = child.Split(',').ToList().Take(1).First(),
                            FullName = child
                        };
                        ListOfContainers.Add(ChildOU);
                    }


                }
            }
            catch (LdapException ldapException)
            {
                if (ldapException.ErrorCode.Equals(ldapErrorInvalidCredentials))
                {
                }
            }

            return new OUInfo
                {
                    CurrentContainer = ADPath,
                    ListOfContainers = ListOfContainers
                };
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

        // POST api/ou
        public void Post([FromBody]string value)
        {
        }

        // PUT api/ou/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/ou/5
        public void Delete(int id)
        {
        }
    }
}
