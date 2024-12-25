using Plugins.Communication.Ethernet.SocketEvent;
using Plugins.Communication.Ethernet.UDP;
using Plugins.Communication.VISA;
using Plugins.Communication.VISA.Model;
using Plugins.Communication.VISA.VXI11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Plugins.Communication
{
    public class FindDeviceService
    {
        private static FindDeviceService instance;
        // 定义一个标识确保线程同步
        private static readonly object locker = new object();
        private static readonly object dataLocker = new object();

        private List<string> lanAddress;
        private List<string> usbAddress;
        private List<string> comAddress;

        private const string UdpTargetIp = "255.255.255.255";
        private const int UdpTargetPort = 111;
        private const int XIDNum = 0x000003ea;
        private const int CallMessageType = 0x00;
        private const int RPCVersionNum = 0x02;
        private const int ProgramNum = 0x000186a0;
        private const int ProgramVersionNum = 0x02;
        private const int GetPortProcedure = 0x03;
        private const int VXI11Core = 0x000607af;

        private DriveIO driveIO;

        private List<InstrucmentInfo> _AllInstrucments;
        /// <summary>
        /// 所有设备
        /// </summary>
        public List<InstrucmentInfo> AllInstrucments
        {
            get
            {
                _AllInstrucments = USBInstrucments.Concat(LANInstrucments).Concat(COMInstrucments).ToList();

                return _AllInstrucments;
            }

            set
            {
                if (_AllInstrucments != value)
                {
                    _AllInstrucments = value;
                }

            }

        }

        private List<InstrucmentInfo> _LANInstrucments;
        /// <summary>
        /// LAN设备
        /// </summary>
        public List<InstrucmentInfo> LANInstrucments
        {
            get
            {
                lock (dataLocker)
                {
                    return _LANInstrucments;
                }

            }
            set
            {
                if (_LANInstrucments != value)
                {
                    _LANInstrucments = value;
                }

            }
        }

        private List<InstrucmentInfo> _USBInstrucments;
        /// <summary>
        /// USB设备
        /// </summary>
        public List<InstrucmentInfo> USBInstrucments
        {
            get { return _USBInstrucments; }
            set
            {
                if (_USBInstrucments != value)
                {
                    _USBInstrucments = value;
                }

            }
        }

        private List<InstrucmentInfo> _COMInstrucments;
        /// <summary>
        /// COM设备
        /// </summary>
        public List<InstrucmentInfo> COMInstrucments
        {
            get { return _COMInstrucments; }
            set
            {
                if (_COMInstrucments != value)
                {
                    _COMInstrucments = value;
                }

            }
        }

        public FindDeviceService()
        {
            AllInstrucments = new List<InstrucmentInfo>();
            LANInstrucments = new List<InstrucmentInfo>();
            USBInstrucments = new List<InstrucmentInfo>();
            COMInstrucments = new List<InstrucmentInfo>();

            lanAddress = new List<string>();
            usbAddress = new List<string>();
            comAddress = new List<string>();

        }

        public void InitDriveIO()
        {
            // 创建DriveIO对象
            driveIO = new DriveIO();
        }

        public void CloseDriveIO()
        {
            if (driveIO != null)
            {
                driveIO.Dispose();
            }
        }

        /// <summary>
        /// 发送找寻符合VXI-11\LXI协议设备命令，使用网络端口loaclPort
        /// </summary>
        /// <param name="loaclPort">本地端口</param>
        public void ScanLANInstrucments(int loaclPort, string ip = "")
        {
            lock (dataLocker)
            {
                LANInstrucments = new List<InstrucmentInfo>();

                UDPClient client = new UDPClient(loaclPort, UdpTargetPort);
                client.Connect(ip);
                client.ReceiveEve += UDPClient_ReceiveEve;

                //VXI-11 portmap
                RemoteProcedureCall remoteProcedureCall = new RemoteProcedureCall()
                {
                    XID = XIDNum,
                    MessageType = CallMessageType,
                    RPCVersion = RPCVersionNum,
                    Program = ProgramNum,
                    ProgramVersion = ProgramVersionNum,
                    Procedure = GetPortProcedure,
                    Credentials = new Credential()
                    {
                        Flavor = 0,
                        Length = 0
                    },
                    Verifiers = new Verifier()
                    {
                        Flavor = 0,
                        Length = 0
                    },
                    VXI = new VXI11Protocol()
                    {
                        Program = VXI11Core,//vxi-11 core
                        Version = 1,
                        Proto = (int)ProtocolType.Tcp,
                        Port = 0
                    }
                };

                byte[] byteArray = remoteProcedureCall.ToBytes();

                client.Send(UdpTargetIp, byteArray);

                //留出时间等待局域网中设备回复
                Thread.Sleep(500);
                client.Close();

                GetInstrInfo(InstrucmentInterface.LAN);
            }
        }

        public void ScanUSBInstrucments()
        {
            lock (dataLocker)
            {
                USBInstrucments = new List<InstrucmentInfo>();

                usbAddress = driveIO.FindResources(FilterDescriptionType.USB);
                GetInstrInfo(InstrucmentInterface.USB);
            }
        }

        public void ScanCOMInstrucments()
        {
            lock (dataLocker)
            {
                COMInstrucments = new List<InstrucmentInfo>();

                comAddress = driveIO.FindResources(FilterDescriptionType.Serial);
                GetInstrInfo(InstrucmentInterface.COM);
            }
        }

        private void UDPClient_ReceiveEve(object sender, ReceiveEvent e)
        {

            string address = $"TCPIP0::{e.IP}::inst0::INSTR";
            if (!lanAddress.Contains(address))
            {
                lanAddress.Add(address);
            }
        }


        private void GetInstrInfo(InstrucmentInterface instrucmentInterface)
        {
            try
            {
                List<string> addressStrs;
                switch (instrucmentInterface)
                {
                    case InstrucmentInterface.USB:
                        addressStrs = usbAddress;
                        break;
                    case InstrucmentInterface.LAN:
                        addressStrs = lanAddress;
                        break;
                    case InstrucmentInterface.COM:
                        addressStrs = comAddress;
                        break;
                    default:
                        addressStrs = new List<string>();
                        break;
                }

                foreach (var address in addressStrs)
                {
                    if (driveIO.OpenResource(address))
                    {
                        // 与设备通信
                        driveIO.WriteLine("*IDN?\n");
                        string text = driveIO.Read();

                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                        {
                            text = text.Trim();
                            var arrays = text.Split(',');

                            // 确保返回的信息包含所需的四个元素
                            if (arrays.Length == 4)
                            {
                                InstrucmentInfo info = new InstrucmentInfo()
                                {
                                    Address = address,
                                    ComType = COMType.VISA,
                                    Interface = instrucmentInterface,
                                    Manufacturer = arrays[0],
                                    Model = arrays[1],
                                    SerialNumber = arrays[2],
                                    Version = arrays[3],
                                    Description = text
                                };

                                //ADD INFO
                                switch (instrucmentInterface)
                                {
                                    case InstrucmentInterface.USB:
                                        // 检查是否已存在该信息，若不存在则添加
                                        if (!USBInstrucments.Contains(info))
                                        {
                                            USBInstrucments.Add(info);
                                        }
                                        break;
                                    case InstrucmentInterface.LAN:
                                        // 检查是否已存在该信息，若不存在则添加
                                        if (!LANInstrucments.Contains(info))
                                        {
                                            LANInstrucments.Add(info);
                                        }
                                        break;
                                    case InstrucmentInterface.COM:
                                        // 检查是否已存在该信息，若不存在则添加
                                        if (!COMInstrucments.Contains(info))
                                        {
                                            COMInstrucments.Add(info);
                                        }
                                        break;
                                    default:
                                        break;
                                }

                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                // 捕获异常并输出调试信息
                Debug.WriteLine($"ex:{ex}");
            }
        }

        public static FindDeviceService GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new FindDeviceService();
                    }
                }
            }

            return instance;
        }
    }
}
