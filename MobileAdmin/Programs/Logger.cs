using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileAdmin
{

    public class Logger
    {
        private IHubContext _hub;
        private string _status;

        public Logger(IHubContext hub)
        {
            _hub = hub;
        }

        public void Write(string output)
        {
            _hub.Clients.All.updateStatus(output);
        }

        public void Write(string output, string id)
        {
            _hub.Clients.All.updateStatus(output, id);
        }

        public void UpdateStatus(string output, string id)
        {
            _status = output;
            this.Write(output, id);
        }

        public string GetStatus()
        {
            return _status;
        }
    }
}