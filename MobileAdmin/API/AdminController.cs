using MobileAdmin.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileAdmin.API
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AdminController : ApiController
    {
        // GET api/admin
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/admin/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/admin
        public IEnumerable<string> Post([FromBody]Computer value)
        {
            List<string> accounts = new List<string>();

            using (DirectoryEntry machine = new DirectoryEntry("WinNT://" + value.IPAddress))
            {
                using (DirectoryEntry group = machine.Children.Find("Administrators", "Group"))
                {
                    object members = group.Invoke("Members", null);

                    foreach (var member in (IEnumerable)members)
                    {
                        DirectoryEntry entry = new DirectoryEntry(member);
                        string accountName = entry.Path.Substring(8, entry.Path.Length - 8);
                        accounts.Add(accountName);
                    }
                }
            }

            return accounts;
        }

        // PUT api/admin/5?action="delete"OR"add"
        public void Put(string id, string action, [FromBody]Computer comp)
        {
            if (action.Equals("delete"))
            {
                RemoveAdminFromComputer(id, comp);
            }
            else if (action.Equals("add"))
            {
                AddAdminToComputer(id, comp);
            }

            
        }

        private void AddAdminToComputer(string id, Computer comp)
        {
            using (DirectoryEntry machine = new DirectoryEntry("WinNT://" + comp.IPAddress))
            {
                using (DirectoryEntry group = machine.Children.Find("Administrators", "Group"))
                {
                    group.Invoke("Add", new[] { "WinNT://" + id });

                    group.CommitChanges();
                }
            }
        }

        private void RemoveAdminFromComputer(string id, Computer comp)
        {
            using (DirectoryEntry machine = new DirectoryEntry("WinNT://" + comp.IPAddress))
            {
                using (DirectoryEntry group = machine.Children.Find("Administrators", "Group"))
                {
                    object members = group.Invoke("Members", null);

                    foreach (var member in (IEnumerable)members)
                    {
                        DirectoryEntry entry = new DirectoryEntry(member);
                        string accountName = entry.Path.Substring(8, entry.Path.Length - 8);
                        if (accountName == id)
                        {
                            group.Invoke("Remove", new[] { entry.Path });
                        }
                    }

                    group.CommitChanges();
                }
            }
        }

        // DELETE api/admin/5
        public void Delete(string id, [FromBody]Computer value)
        {
            
        }
    }
}
