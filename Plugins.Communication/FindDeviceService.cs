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
        private List<string> gpibAddress;

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
                _AllInstrucments = USBInstrucments.Concat(LANInstrucments).Concat(COMInstrucments).Concat(GPIBInstrucments).ToList();

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

        private List<InstrucmentInfo> _GPIBInstrucments;
        /// <summary>
        /// GPIB设备
        /// </summary>
        public List<InstrucmentInfo> GPIBInstrucments
        {
            get { return _GPIBInstrucments; }
            set
            {
                if (_GPIBInstrucments != value)
                {
                    _GPIBInstrucments = value;
                }

            }
        }

        public FindDeviceService()
        {
            AllInstrucments = new List<InstrucmentInfo>();
            LANInstrucments = new List<InstrucmentInfo>();
            USBInstrucments = new List<InstrucmentInfo>();
            COMInstrucments = new List<InstrucmentInfo>();
            GPIBInstrucments = new List<InstrucmentInfo>();

            lanAddress = new List<string>();
            usbAddress = new List<string>();
            comAddress = new List<string>();
            gpibAddress = new List<string>();
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

        /// <summary>
        /// 扫描usb设备
        /// </summary>
        public void ScanUSBInstrucments()
        {
            lock (dataLocker)
            {
                USBInstrucments = new List<InstrucmentInfo>();

                usbAddress = driveIO.FindResources(FilterDescriptionType.USB);
                GetInstrInfo(InstrucmentInterface.USB);
            }
        }

        /// <summary>
        /// 扫描COM设备
        /// </summary>
        public void ScanCOMInstrucments()
        {
            lock (dataLocker)
            {
                COMInstrucments = new List<InstrucmentInfo>();

                comAddress = driveIO.FindResources(FilterDescriptionType.Serial);
                GetInstrInfo(InstrucmentInterface.COM);
            }
        }

        /// <summary>
        /// 扫描GPIB设备
        /// </summary>
        public void ScanGPIBInstrucments()
        {
            lock (dataLocker)
            {
                GPIBInstrucments = new List<InstrucmentInfo>();

                gpibAddress = driveIO.FindResources(FilterDescriptionType.GPIB);
                GetInstrInfo(InstrucmentInterface.GPIB);
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
                // 使用字典来映射接口类型到对应的地址列表
                var addressDictionary = new Dictionary<InstrucmentInterface, List<string>>()
                {
                    { InstrucmentInterface.USB, usbAddress },
                    { InstrucmentInterface.LAN, lanAddress },
                    { InstrucmentInterface.COM, comAddress },
                    { InstrucmentInterface.GPIB, gpibAddress }
                };

                // 获取对应的地址列表，如果没有找到，则返回空列表
                if (!addressDictionary.TryGetValue(instrucmentInterface, out var addressStrs))
                {
                    addressStrs = new List<string>();
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

                                // 使用字典映射来简化存储设备信息的逻辑
                                var instrumentListDictionary = new Dictionary<InstrucmentInterface, List<InstrucmentInfo>>()
                                {
                                    { InstrucmentInterface.USB, USBInstrucments },
                                    { InstrucmentInterface.LAN, LANInstrucments },
                                    { InstrucmentInterface.COM, COMInstrucments },
                                    { InstrucmentInterface.GPIB, GPIBInstrucments }
                                };

                                if (instrumentListDictionary.ContainsKey(instrucmentInterface))
                                {
                                    var instrumentList = instrumentListDictionary[instrucmentInterface];

                                    // 检查是否已存在该信息，若不存在则添加
                                    if (!instrumentList.Contains(info))
                                    {
                                        instrumentList.Add(info);
                                    }
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
