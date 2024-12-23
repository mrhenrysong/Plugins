using System;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class DisconnectionEvent : EventArgs
    {
        public string AddressPort
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public DisconnectionEvent(string sockip, Exception exception)
        {
            this.AddressPort = sockip;
            this.Exception = exception;
        }
    }
}
