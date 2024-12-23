using Plugins.Communication.Ethernet.SocketEvent;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugins.Communication.Ethernet.SocketList
{
    public class SockList : IEnumerable
    {
        private ConcurrentDictionary<string, WebSocket> dictionary = new ConcurrentDictionary<string, WebSocket>();

        private static object lockObj = new object();
        public WebSocket this[string index]
        {
            get
            {
                lock (lockObj)
                {
                    if (this.dictionary.ContainsKey(index))
                    {
                        return this.dictionary[index];
                    }
                    return null;
                }
            }
            set
            {
                this.dictionary[index] = value;
            }
        }

        public event Action<object, SockListDisconnectionEvent> DisconnectionEvent;

        public event Action<object, SockListReceiveEvent> ReceiveEvent;

        public bool Remove(string ID)
        {
            WebSocket temp = default(WebSocket);
            return this.dictionary.TryRemove(ID, out temp);
        }

        public bool WebReceive(string index)
        {
            if (this.dictionary[index].First)
            {
                if (this.dictionary.Count > 0)
                {
                    try
                    {
                        byte[] by = new byte[10000];
                        int a = this.dictionary[index].Socket.Receive(by, 0, by.Length, SocketFlags.None);
                        if (a != 0)
                        {
                            byte[] s3 = by.Take(a).ToArray();
                            this.dictionary[index].Socket.Send(SockList.PackHandShakeData(SockList.GetSecKeyAccetp(s3, s3.Length)));
                            this.dictionary[index].First = false;
                            return true;
                        }
                        this.dictionary[index].Socket.Close();
                        throw new SocketException();
                    }
                    catch (SocketException e2)
                    {
                        this.DisconnectionEvent(this, new SockListDisconnectionEvent(index, e2));
                        return false;
                    }
                }
                return false;
            }
            if (this.dictionary.Count > 0)
            {
                try
                {
                    byte[] by2 = new byte[10000];
                    int a2 = this.dictionary[index].Socket.Receive(by2, 0, by2.Length, SocketFlags.None);
                    if (a2 != 0)
                    {
                        byte[] s2 = by2.Take(a2).ToArray();
                        s2 = this.AnalyticData(s2, s2.Length);
                        if (s2.Length != 0)
                        {
                            this.ReceiveEvent(this, new SockListReceiveEvent(index, s2));
                        }
                        return true;
                    }
                    this.dictionary[index].Socket.Close();
                    throw new SocketException();
                }
                catch (SocketException e)
                {
                    this.DisconnectionEvent(this, new SockListDisconnectionEvent(index, e));
                    return false;
                }
            }
            return false;
        }

        private byte[] AnalyticData(byte[] recBytes, int recByteLength)
        {
            if (recByteLength < 2)
            {
                return null;
            }
            if ((recBytes[0] & 128) != 128)
            {
                return null;
            }
            if ((recBytes[1] & 128) != 128)
            {
                return null;
            }
            int payload_len = recBytes[1] & 127;
            byte[] masks = new byte[4];
            byte[] payload_data;
            switch (payload_len)
            {
                case 126:
                    Array.Copy(recBytes, 4, masks, 0, 4);
                    payload_len = (ushort)(recBytes[2] << 8 | recBytes[3]);
                    payload_data = new byte[payload_len];
                    Array.Copy(recBytes, 8, payload_data, 0, payload_len);
                    break;
                case 127:
                    {
                        Array.Copy(recBytes, 10, masks, 0, 4);
                        byte[] uInt64Bytes = new byte[8];
                        for (int k = 0; k < 8; k++)
                        {
                            uInt64Bytes[k] = recBytes[9 - k];
                        }
                        ulong len = BitConverter.ToUInt64(uInt64Bytes, 0);
                        payload_data = new byte[len];
                        for (ulong j = 0uL; j < len; j++)
                        {
                            payload_data[j] = recBytes[j + 14];
                        }
                        break;
                    }
                default:
                    Array.Copy(recBytes, 2, masks, 0, 4);
                    payload_data = new byte[payload_len];
                    Array.Copy(recBytes, 6, payload_data, 0, payload_len);
                    break;
            }
            for (int i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }
            return payload_data;
        }

        private static byte[] PackData(byte[] message)
        {
            byte[] contentBytes = null;
            if (message.Length < 126)
            {
                contentBytes = new byte[message.Length + 2];
                contentBytes[0] = 129;
                contentBytes[1] = (byte)message.Length;
                Array.Copy(message, 0, contentBytes, 2, message.Length);
            }
            else if (message.Length < 65535)
            {
                contentBytes = new byte[message.Length + 4];
                contentBytes[0] = 129;
                contentBytes[1] = 126;
                contentBytes[2] = (byte)(message.Length & 255);
                contentBytes[3] = (byte)(message.Length >> 8 & 255);
                Array.Copy(message, 0, contentBytes, 4, message.Length);
            }
            return contentBytes;
        }

        private static byte[] PackHandShakeData(string secKeyAccept)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + Environment.NewLine);
            responseBuilder.Append("Upgrade: websocket" + Environment.NewLine);
            responseBuilder.Append("Connection: Upgrade" + Environment.NewLine);
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKeyAccept + Environment.NewLine + Environment.NewLine);
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        private static string GetSecKeyAccetp(byte[] handShakeBytes, int bytesLength)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, bytesLength);
            string key = string.Empty;
            Regex r = new Regex("Sec\\-WebSocket\\-Key:(.*?)\\r\\n");
            Match i = r.Match(handShakeText);
            if (i.Groups.Count != 0)
            {
                key = Regex.Replace(i.Value, "Sec\\-WebSocket\\-Key:(.*?)\\r\\n", "$1").Trim();
            }
            byte[] encryptionString = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            return Convert.ToBase64String(encryptionString);
        }

        public bool Receive(string index)
        {
            if (this.dictionary.Count > 0)
            {
                try
                {
                    byte[] by = new byte[1024];
                    if (!dictionary.ContainsKey(index) || !dictionary[index].Socket.Connected)
                    {
                        WebSocket temp = default(WebSocket);
                        this.dictionary.TryRemove(index, out temp);
                        return false;
                    }

                    int a = dictionary[index].Socket.Receive(by, 0, by.Length, SocketFlags.None);
                    if (a != 0)
                    {
                        byte[] s = by.Take(a).ToArray();
                        this.ReceiveEvent(this, new SockListReceiveEvent(index, s));
                        return true;
                    }
                    this.dictionary[index].Socket.Close();
                    throw new SocketException();
                }
                catch (SocketException e)
                {
                    this.DisconnectionEvent(this, new SockListDisconnectionEvent(index, e));
                    return false;
                }
            }
            return false;
        }

        public bool Contains(string key)
        {
            return this.dictionary.Keys.Contains(key);
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool WebSend(string key, byte[] by)
        {
            if (by == null || by.Length == 0)
            {
                return false;
            }
            if (this.dictionary.Count > 0)
            {
                if (this.dictionary.Keys.Contains(key))
                {
                    try
                    {
                        int result = this.dictionary[key].Socket.Send(SockList.PackData(by));
                        return true;
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }
        public bool TestSend(int i, byte[] by)
        {
            if (by == null || by.Length == 0)
            {
                return false;
            }
            if (this.dictionary.Count > 0 && this.dictionary.Count >= i + 1)
            {
                try
                {
                    int result = dictionary.ElementAt(i).Value.Socket.Send(by);
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        public bool Send(string key, byte[] by)
        {
            if (by == null || by.Length == 0)
            {
                return false;
            }
            if (this.dictionary.Count > 0)
            {
                if (this.dictionary.Keys.Contains(key))
                {
                    try
                    {
                        int result = this.dictionary[key].Socket.Send(by);
                        return true;
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        public bool Send(IPEndPoint iPEndPoint, byte[] by)
        {
            if (by == null || by.Length == 0)
            {
                return false;
            }
            if (this.dictionary.Count > 0)
            {
                var key = iPEndPoint.ToString();
                if (this.dictionary.Keys.Contains(key))
                {
                    try
                    {
                        int result = this.dictionary[key].Socket.Send(by);
                        return true;
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;

        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < this.dictionary.Values.Count; i++)
            {
                yield return (object)this.dictionary.Values.ElementAt(i);
            }
        }
    }
}
