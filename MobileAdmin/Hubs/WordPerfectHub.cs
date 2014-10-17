using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace MobileAdmin
{
    [HubName("WordPerfectHub")]
    public class WordPerfectHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}