using System;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class ConnectedEvent : EventArgs
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

        public bool Connected
        {
            get;
            set;
        }

        public ConnectedEvent(string Address, Exception exception, bool connected)
        {
            this.AddressPort = Address;
            this.Exception = exception;
            this.Connected = connected;
        }
    }
}
