using Plugins.Communication.Ethernet.SocketEvent;
using System;
using System.Net.Sockets;

namespace Plugins.Communication.Ethernet.TCP
{
    public class TCPClient : IDisposable
    {
        private delegate bool StartReceive(Socket socket);

        private TcpClient Tcp;

        private Socket Socket;

        private string IP;

        private int Port;

        public bool ConnectCondition
        {
            get;
            set;
        }

        public event EventHandler<ConnectedEvent> Connected;

        public event EventHandler<DisconnectionEvent> DisconnectionEve;

        public event EventHandler<ReceiveEvent> ReceiveEve;

        public TCPClient(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public void Connect()
        {
            if (!this.ConnectCondition)
            {
                try
                {
                    if (this.Tcp == null)
                    {
                        this.Tcp = new TcpClient(this.IP, this.Port);
                        this.Socket = this.Tcp.Client;
                        this.ConnectCondition = true;
                        this.Connected(this, new ConnectedEvent(this.IP + ":" + this.Port, null, this.ConnectCondition));
                        StartReceive sr = this.Receive;
                        sr.BeginInvoke(this.Socket, this.rective, this.Socket);
                    }
                    else
                    {
                        this.Tcp.Close();
                        this.Tcp = new TcpClient(this.IP, this.Port);
                        this.Socket = this.Tcp.Client;
                        this.ConnectCondition = true;
                        this.Connected(this, new ConnectedEvent(this.IP + ":" + this.Port, null, this.ConnectCondition));
                        StartReceive sr2 = this.Receive;
                        sr2.BeginInvoke(this.Socket, this.rective, this.Socket);
                    }
                }
                catch (SocketException e)
                {
                    this.ConnectCondition = false;
                    this.Connected(this, new ConnectedEvent(this.IP + ":" + this.Port, e, this.ConnectCondition));
                }
            }
        }

        public bool Send(byte[] by)
        {
            //if (this.ConnectCondition)
            //{
            //	this.Socket.Send(by);
            //	return true;
            //}
            //return false;
            this.Socket.Send(by);
            return true;
        }

        private void rective(IAsyncResult ia)
        {
            if (this.ConnectCondition)
            {
                StartReceive StartReceive = this.Receive;
                StartReceive.BeginInvoke((Socket)ia.AsyncState, this.rective, (Socket)ia.AsyncState);
            }
        }

        private bool Receive(Socket socket)
        {
            try
            {
                byte[] by = new byte[1024];
                if (socket.Receive(by, 0, by.Length, SocketFlags.None) != 0)
                {
                    this.ReceiveEve(this, new ReceiveEvent(this.IP + ":" + this.Port, by, ProtocolType.Tcp));
                    return true;
                }
                socket.Close();
                this.ConnectCondition = false;
                throw new SocketException();
            }
            catch (SocketException e)
            {
                if (this.DisconnectionEve != null)
                {
                    this.DisconnectionEve(this, new DisconnectionEvent(this.IP + ":" + this.Port, e));
                    //return false;
                }
                this.ConnectCondition = false;
                return false;
            }
        }

        public void Dispose()
        {
            this.Socket.Close();
            this.Tcp.Close();
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            this.Socket.Close();
            this.ConnectCondition = false;
        }
    }
}
