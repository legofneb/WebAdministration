using MobileAdmin.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileAdmin.API
{
    [Authorize]
    [InitializeSimpleMembership]
    public class MapDriveController : ApiController
    {
        // GET api/mapdrive
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/mapdrive/IPAddress
        public List<string> Get(string id)
        {
            return GetMappedDrives(id);
        }

        private List<string> GetMappedDrives( string IPAddress )
        {
            List<string> toReturn = new List<string>();

            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation =
            System.Management.ImpersonationLevel.Impersonate;

            //Path for the query, specifying which computer to use
            ManagementPath basePath = new ManagementPath( @"\\" + IPAddress +
            @"\ROOT\CIMV2" );

            //Select all logical drives that are of type 4 which is a
            //network drive
            //Other choices would be:
            //0 Unknown
            //1 No Root Directory
            //2 Removable Disk
            //3 Local Disk
            //4 Network Drive
            //5 Compact Disc
            //6 RAM Disk
            ObjectQuery query = new ObjectQuery( "SELECT * FROM Win32_LogicalDisk WHERE DriveType = 4" );

            ManagementScope scope = new ManagementScope( basePath, options );
            ManagementObjectSearcher searcher = new
            ManagementObjectSearcher( scope, query );

            //Loop through returned drives
            foreach (ManagementObject drive in searcher.Get())
            {
                toReturn.Add(drive.Properties["Name"].Value.ToString());
            }

            return toReturn;
        }

        // POST api/mapdrive
        public void Post([FromBody]string value)
        {
        }

        // PUT api/mapdrive/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/mapdrive/5
        public void Delete(int id)
        {
        }
    }
}
