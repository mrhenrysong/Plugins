using Plugins.Communication.Ethernet.SocketEvent;
using Plugins.Communication.Ethernet.SocketList;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Plugins.Communication.Ethernet.UDP
{
    public class UDPClient : IDisposable
    {
        private delegate bool StartReceive(Socket socket);

        public SockList sockList = new SockList();

        private UdpClient UDP;

        private Socket Socket;


        public int Port
        {
            get;
            set;
        }

        public int SendPort
        {
            get;
            set;
        }

        public bool ConnectCondition
        {
            get;
            private set;
        }

        //public event EventHandler<ConnectedEvent> Link;

        public event EventHandler<DisconnectionEvent> DisconnectionEve;

        public event EventHandler<ReceiveEvent> ReceiveEve;

        public UDPClient(int localport, int remotePort)
        {
            this.SendPort = remotePort;
            this.Port = localport;
        }

        public UDPClient()
        {
        }

        public void Connect(string ip = "")
        {
            if (!this.ConnectCondition)
            {
                try
                {
                    IPEndPoint ied;
                    if (ip == "")
                    {
                        ied = new IPEndPoint(IPAddress.Any, this.Port);
                    }
                    else
                    {
                        ied = new IPEndPoint(IPAddress.Parse(ip), this.Port);
                    }

                    this.UDP = new UdpClient(ied);
                    this.Socket = this.UDP.Client;
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    Socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                    this.Socket.EnableBroadcast = true;
                    this.Socket.DontFragment = true;
                    this.Socket.ReceiveTimeout = 5000;
                    this.ConnectCondition = true;
                    this.UDP.BeginReceive(this.AsyncRective, this.UDP);
                }
                catch (SocketException)
                {
                    this.ConnectCondition = false;
                }
            }
        }
        public bool Send(string ip, byte[] by)
        {
            if (this.ConnectCondition)
            {
                int xx = this.Socket.SendTo(by, new IPEndPoint(IPAddress.Parse(ip), this.SendPort));
                return xx == by.Length;
            }
            return false;
        }
        public bool Send(IPEndPoint iPEndPoint, byte[] by)
        {
            if (this.ConnectCondition)
            {
                int xx = this.Socket.SendTo(by, iPEndPoint);
                return xx == by.Length;
            }
            return false;
        }

        private void AsyncRective(IAsyncResult ia)
        {
            if (this.ConnectCondition && ia.IsCompleted)
            {
                try
                {
                    UdpClient udp = (UdpClient)ia.AsyncState;
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, this.Port);
                    byte[] by = udp.EndReceive(ia, ref groupEP);

                    // 触发事件，处理接收到的数据
                    this.ReceiveEve?.Invoke(this, new ReceiveEvent(groupEP.ToString(), by, ProtocolType.Udp));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    // 确保在接收完成后重新开始接收
                    try
                    {
                        if (ConnectCondition)
                        {
                            this.UDP.BeginReceive(this.AsyncRective, this.UDP);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // UDP已关闭，避免重复调用
                        Debug.WriteLine("UDP client has been disposed.");
                    }
                }
            }
        }

        private bool Receive(Socket socket)
        {
            try
            {
                byte[] by = new byte[64];
                int receiveCount = socket.Receive(by, 0, by.Length, SocketFlags.None);
                if (receiveCount != 0)
                {
                    byte[] by2 = by.Take(receiveCount).ToArray();
                    this.ReceiveEve(this, new ReceiveEvent(socket.RemoteEndPoint.ToString(), by2, ProtocolType.Udp));
                    return true;
                }
                return false;
            }
            catch (SocketException e)
            {
                this.DisconnectionEve(this, new DisconnectionEvent(socket.RemoteEndPoint.ToString(), e));
                return false;
            }
            catch
            {
                this.DisconnectionEve(this, new DisconnectionEvent(socket.RemoteEndPoint.ToString(), new Exception()));
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                this.UDP.Close();
                this.ConnectCondition = false;
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void Close()
        {
            this.Socket.Close();
            this.ConnectCondition = false;
        }
    }
}
