using System.Net.Sockets;

namespace Plugins.Communication.Ethernet.SocketList
{
    public class WebSocket
    {
        public Socket Socket
        {
            get;
            set;
        }

        public bool First
        {
            get;
            set;
        }

        public WebSocket(Socket _Socket, bool _First)
        {
            this.First = _First;
            this.Socket = _Socket;
        }
    }
}
