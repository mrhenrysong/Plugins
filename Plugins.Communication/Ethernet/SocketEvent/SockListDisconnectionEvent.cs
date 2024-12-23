using System;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class SockListDisconnectionEvent : EventArgs
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

        public SockListDisconnectionEvent(string sockip, Exception exception)
        {
            this.AddressPort = sockip;
            this.Exception = exception;
        }
    }
}
