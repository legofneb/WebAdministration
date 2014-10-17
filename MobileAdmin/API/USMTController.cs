using Microsoft.AspNet.SignalR;
using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;

namespace MobileAdmin.API
{
    public class USMTComputer
    {
        public string IPAddress { get; set; }
        public string NewIPAddress { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
    }

    [System.Web.Http.Authorize]
    [InitializeSimpleMembership]
    public class USMTController : ApiController
    {

        // GET api/usmt
        public List<USMTComputer> Get()
        {
            USMTList list = USMTList.Instance();
            List<Program> programs = list.GetPrograms();

            List<USMTComputer> comps = programs.Select(n => new USMTComputer
            {
                IPAddress = ((USMT)n)._computer.IPAddress,
                NewIPAddress = ((USMT)n)._computer.NewIPAddress,
                User = ((USMT)n)._computer.USMTGID,
                Status = ((USMT)n).GetStatus()
            }).ToList();

            return comps;
        }

        // GET api/usmt/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/usmt
        public void Post([FromBody]Computer value)
        {
            USMTList list = USMTList.Instance();
            
            USMT USMT;

            if (!value.AttachUSMT)
            {
                USMT = new USMT(value, USMTMethod.Full);
            }
            else
            {
                USMT = new USMT(value, USMTMethod.Attach);
            }

            list.AddProgram(value.NewIPAddress, USMT);
            list.StartProgram(value.NewIPAddress);
        }

        // PUT api/usmt/5
        public void Put([FromBody]Computer value)
        {
        }

        // DELETE api/usmt/5
        public void Delete(int id)
        {
        }
    }
}
