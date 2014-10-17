using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace MobileAdmin
{
    [HubName("TSMHub")]
    public class TSMHub : Hub
    {

        public void Status(string update)
        {
            Clients.All.updateStatus(update);
        }
    }
}