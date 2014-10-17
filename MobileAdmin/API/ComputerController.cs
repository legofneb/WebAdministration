using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using aulease.Entities;
using MobileAdmin.Filters;

namespace MobileAdmin.API
{
    [Authorize]
    [InitializeSimpleMembership]
    public class ComputerController : ApiController
    {
        // GET api/computer
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/computer/leaseTagORserialNumber
        public Computer Get(string id)
        {
            string value = id;

            AuleaseEntities db = new AuleaseEntities();
            Component comp;
            IPAddress parse;

            if (db.Components.Any(n => n.SerialNumber == value))
            {
                comp = db.Components.Where(n => n.SerialNumber == value).Single();
            }
            else if (db.Components.Any(n => n.LeaseTag == value))
            {
                comp = db.Components.Where(n => n.LeaseTag == value).Single();
            }
            else if (db.Components.Any(n => n.ComputerName == value))
            {
                comp = db.Components.Where(n => n.ComputerName == value).Single();
            }
            else if (IPAddress.TryParse(value, out parse))
            {
                comp = new Component() { IPAddress = value };
            }
            else
            {
                comp = new Component();
            }

            if (comp.Id == null)
            {
                //couldn't find component....
                return null;
            }

            return new Computer
            {
                SerialNumber = comp.SerialNumber,
                LeaseTag = comp.LeaseTag,
                IPAddress = comp.IPAddress,
                ComputerName = comp.ComputerName,
                MAC = comp.MAC,
                AdminCredentials = (comp.Leases.Count > 0) ? Domain.AddDomainToUsername(comp.GID()) : ""
            };

        }

        // POST api/computer
        public void Post([FromBody]string value)
        {
        }

        // PUT api/computer/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/computer/5
        public void Delete(int id)
        {
        }
    }
}
