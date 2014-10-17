using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{
    public class Computer
    {
        public string SerialNumber { get; set; }
        public string MAC { get; set; }
        public string LeaseTag { get; set; }
        public string IPAddress { get; set; }
        public string ComputerName { get; set; }

        public string AdminCredentials { get; set; }

        public string TSMNodeName { get; set; }

        public string USMTGID { get; set; }
        public bool AttachUSMT { get; set; }


        public string NewIPAddress { get; set; }
    }
}