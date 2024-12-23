using Plugins.Communication.Ethernet.SocketEvent;
using Plugins.Communication.Ethernet.SocketList;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Plugins.Communication.Ethernet.TCP
{
    public class TcpServer : IDisposable
    {
        private delegate bool StartReceive(string IP);

        private bool Loop = true;

        private Thread th;

        private readonly TcpListener tcp;

        public SockList sockList = new SockList();

        public bool Status
        {
            get;
            set;
        }

        public TcpServerEnum TcpServerEnum
        {
            get;
            set;
        }

        public event EventHandler<LinkEvent> Link;

        public event EventHandler<DisconnectionEvent> Disconnection;

        public event EventHandler<ReceiveEvent> Receive;

        public TcpServer(int port, TcpServerEnum _TcpServerEnum)
        {
            this.tcp = new TcpListener(IPAddress.Any, port);

            this.sockList.ReceiveEvent += this.SockList_ReceiveEvent;
            this.sockList.DisconnectionEvent += this.SockList_DisconnectionEvent;
            this.TcpServerEnum = _TcpServerEnum;
            this.Status = false;
        }

        private void SockList_DisconnectionEvent(object sender, SockListDisconnectionEvent e)
        {
            if (this.sockList.Contains(e.AddressPort))
            {
                this.sockList[e.AddressPort].Socket.Shutdown(SocketShutdown.Both);
                this.sockList[e.AddressPort].Socket.Close();
                this.sockList.Remove(e.AddressPort);
            }
            this.Disconnection(this, new DisconnectionEvent(e.AddressPort, e.Exception));
        }

        private void SockList_ReceiveEvent(object sender, SockListReceiveEvent e)
        {
            this.Receive(this, new ReceiveEvent(e.AddressPort, e.Message, ProtocolType.Tcp));
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        private void Accept()
        {
            while (this.Loop)
            {
                Socket socket = this.tcp.AcceptSocket();
                int keepAlive = -1744830460;
                byte[] inValue = new byte[12]
                {
                    1,
                    0,
                    0,
                    0,
                    232,
                    3,
                    0,
                    0,
                    232,
                    3,
                    0,
                    0
                };
                socket.IOControl(keepAlive, inValue, null);
                LingerOption linerOption = new LingerOption(false, 0);
                socket.LingerState = linerOption;
                if (socket != null && socket.Connected)
                {

                    // 设置 SO_LINGER 套接字选项
                    LingerOption lingerOption = new LingerOption(true, 1);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    this.sockList[socket.RemoteEndPoint.ToString()] = new WebSocket(socket, true);
                    if (this.Link != null)
                    {
                        this.Link(this, new LinkEvent(socket.RemoteEndPoint.ToString(), true, DateTime.Now, ProtocolType.Tcp));
                    }
                    StartReceive StartReceive = null;
                    if (this.TcpServerEnum == TcpServerEnum.NetSocket)
                    {
                        StartReceive = this.sockList.Receive;
                    }
                    else if (this.TcpServerEnum == TcpServerEnum.WebSocket)
                    {
                        StartReceive = this.sockList.WebReceive;
                    }
                    if (StartReceive != null)
                    {
                        if (socket != null && socket.Connected)
                        {
                            try
                            {
                                StartReceive.BeginInvoke(socket.RemoteEndPoint.ToString(), this.rective, socket.RemoteEndPoint.ToString());
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void rective(IAsyncResult ia)
        {
            if (this.sockList.Contains((string)ia.AsyncState))
            {
                StartReceive StartReceive = null;
                if (this.TcpServerEnum == TcpServerEnum.NetSocket)
                {
                    StartReceive = this.sockList.Receive;
                }
                else if (this.TcpServerEnum == TcpServerEnum.WebSocket)
                {
                    StartReceive = this.sockList.WebReceive;
                }
                if (StartReceive != null)
                {
                    try
                    {
                        StartReceive.BeginInvoke((string)ia.AsyncState, this.rective, (string)ia.AsyncState);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        public void Close(string ip)
        {
            if (this.sockList.Contains(ip))
            {
                this.sockList[ip].Socket.Close();
                this.sockList.Remove(ip);
            }
        }
        public bool TestSend(int i, byte[] stream)
        {

            return this.sockList.TestSend(i, stream);
        }

        public bool Send(string ip, byte[] by)
        {
            if (this.TcpServerEnum == TcpServerEnum.NetSocket)
            {
                return this.sockList.Send(ip, by);
            }
            return this.sockList.WebSend(ip, by);
        }
        public bool Send(IPEndPoint iPEndPoint, byte[] by)
        {
            if (this.TcpServerEnum == TcpServerEnum.NetSocket)
            {
                return this.sockList.Send(iPEndPoint, by);
            }
            return this.sockList.Send(iPEndPoint, by);
        }
        public void Start()
        {
            this.tcp.Start();

            this.th = new Thread(this.Accept);
            this.th.IsBackground = true;
            this.Loop = true;
            this.Status = true;
            this.th.Start();
        }

        public void Dispose()
        {
            Loop = false;
            if (this.th.IsAlive)
            {
                foreach (Socket sock in this.sockList)
                {
                    try
                    {
                        sock.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        sock.Close(100);
                        sock.Dispose();
                    }

                }
                this.Close();
                this.th.Abort();
                try
                {
                    this.tcp.AcceptSocket().Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    this.tcp.AcceptSocket().Close(100);
                    this.tcp.AcceptSocket().Dispose();
                }
                this.tcp.Stop();
                GC.Collect();
            }
        }

        public void Close()
        {
            this.Status = false;
            this.Loop = false;
        }
    }
}
