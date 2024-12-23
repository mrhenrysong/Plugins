using System;
using System.Net.Sockets;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class LinkEvent : EventArgs
    {
        private string _Address;

        private DateTime _Time;

        public ProtocolType ProtocolType
        {
            get;
            set;
        }

        public bool Connected
        {
            get;
            set;
        }

        public string Address
        {
            get
            {
                return this._Address;
            }
            set
            {
                this._Address = value;
            }
        }

        public DateTime Time
        {
            get
            {
                return this._Time;
            }
            set
            {
                this._Time = value;
            }
        }

        public LinkEvent(string address, bool connected, DateTime time, ProtocolType protocolType)
        {
            this.ProtocolType = protocolType;
            this.Connected = connected;
            this.Address = address;
            this.Time = time;
        }
    }
}
