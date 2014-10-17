using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileAdmin.API
{
    [Authorize]
    [InitializeSimpleMembership]
    public class IDFController : ApiController
    {
        // GET api/idf
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/idf/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/idf
        public void Post([FromBody]Computer value)
        {
            IDF idf = new IDF(value);
            idf.Run();
        }

        // PUT api/idf/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/idf/5
        public void Delete(int id)
        {
        }
    }
}
