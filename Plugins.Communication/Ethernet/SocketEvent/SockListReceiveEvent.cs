using System;

namespace Plugins.Communication.Ethernet.SocketEvent
{
    public class SockListReceiveEvent : EventArgs
    {
        public string AddressPort;

        public byte[] Message;

        public SockListReceiveEvent(string IP, byte[] By)
        {
            this.AddressPort = IP;
            this.Message = By;
        }
    }
}
