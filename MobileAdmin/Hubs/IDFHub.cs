﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{
    [HubName("IDFHub")]
    public class IDFHub : Hub
    {

        public void Status(string update)
        {
            Clients.All.updateStatus(update);
        }
    }
}