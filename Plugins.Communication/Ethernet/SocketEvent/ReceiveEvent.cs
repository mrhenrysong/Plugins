using System;
using System.Net.Sockets;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class ReceiveEvent : EventArgs
    {
        public ProtocolType ProtocolType
        {
            get;
            set;
        }

        public string IP
        {
            get;
            set;
        }

        public string Port
        {
            get;
            set;
        }

        public string Address
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public ReceiveEvent(string address, byte[] by, ProtocolType protocolType)
        {
            this.ProtocolType = protocolType;
            this.Address = address;
            this.IP = address.Split(':')[0];
            this.Port = address.Split(':')[1];
            this.Data = by;
        }
    }
}
