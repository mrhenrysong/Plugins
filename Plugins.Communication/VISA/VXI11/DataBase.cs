using System;
using System.Linq;

namespace Plugins.Communication.VISA.VXI11
{
    /// <summary>
    /// RemoteProdeceCall结构体
    /// </summary>
    public class RemoteProcedureCall
    {
        /// <summary>
        /// xid 默认1002 0x000003ea
        /// </summary>
        public int XID;

        /// <summary>
        /// 信息类型 call：0
        /// </summary>
        public int MessageType;

        /// <summary>
        /// RPC版本 默认2
        /// </summary>
        public int RPCVersion;

        /// <summary>
        /// 协议类型 默认Portmap:100000 0x000186a0
        /// </summary>
        public int Program;

        /// <summary>
        /// 协议版本 默认2
        /// </summary>
        public int ProgramVersion;

        /// <summary>
        /// 规则 GetPort:3
        /// </summary>
        public int Procedure;

        /// <summary>
        /// 证书
        /// </summary>
        public Credential Credentials;

        /// <summary>
        /// 校验
        /// </summary>
        public Verifier Verifiers;

        /// <summary>
        /// vxi11协议具体包
        /// </summary>
        public VXI11Protocol VXI;

        //byte[] datas = new byte[] { 0x00, 0x00, 0x03, 0xea, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        //        0x02 , 0x00, 0x01 , 0x86 , 0xa0 , 0x00 , 0x00 , 0x00 , 0x02 , 0x00 , 0x00 , 0x00 , 0x03 , 0x00 , 0x00,
        //        0x00, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00
        //        , 0x06 , 0x07 , 0xaf , 0x00 , 0x00 , 0x00 , 0x02 , 0x00 , 0x00 , 0x00 , 0x06 , 0x00 , 0x00 , 0x00 , 0x00 };
        public byte[] ToBytes()
        {
            // Convert XID (int) - 4 bytes
            byte[] xid = BitConverter.GetBytes(this.XID);
            if (BitConverter.IsLittleEndian) Array.Reverse(xid);

            // Convert MessageType (int) - 4 bytes
            byte[] messageType = BitConverter.GetBytes(this.MessageType);
            if (BitConverter.IsLittleEndian) Array.Reverse(messageType);

            // Convert RPCVersion (int) - 4 bytes
            byte[] rpcVersion = BitConverter.GetBytes(this.RPCVersion);
            if (BitConverter.IsLittleEndian) Array.Reverse(rpcVersion);

            // Convert Program (int) - 4 bytes
            byte[] program = BitConverter.GetBytes(this.Program);
            if (BitConverter.IsLittleEndian) Array.Reverse(program);

            // Convert ProgramVersion (int) - 4 bytes
            byte[] programVersion = BitConverter.GetBytes(this.ProgramVersion);
            if (BitConverter.IsLittleEndian) Array.Reverse(programVersion);

            // Convert Procedure (int) - 4 bytes
            byte[] procedure = BitConverter.GetBytes(this.Procedure);
            if (BitConverter.IsLittleEndian) Array.Reverse(procedure);

            // Convert Credentials (Flavor + Length)
            byte[] credentials = BitConverter.GetBytes(this.Credentials.Flavor)
                                        .Concat(BitConverter.GetBytes(this.Credentials.Length))
                                        .ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(credentials);

            // Convert Verifiers (Flavor + Length)
            byte[] verifiers = BitConverter.GetBytes(this.Verifiers.Flavor)
                                          .Concat(BitConverter.GetBytes(this.Verifiers.Length))
                                          .ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(verifiers);

            // Convert VXI (Program + Version + Proto + Port)
            byte[] vxi = BitConverter.GetBytes(this.VXI.Program)
                                    .Concat(BitConverter.GetBytes(this.VXI.Version))
                                    .Concat(BitConverter.GetBytes(this.VXI.Proto))
                                    .Concat(BitConverter.GetBytes(this.VXI.Port))
                                    .ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(vxi);

            // Combine all byte arrays into a final byte array
            return xid.Concat(messageType)
                      .Concat(rpcVersion)
                      .Concat(program)
                      .Concat(programVersion)
                      .Concat(procedure)
                      .Concat(credentials)
                      .Concat(verifiers)
                      .Concat(vxi)
                      .ToArray();
        }
    }

    /// <summary>
    /// 证书
    /// </summary>
    public class Credential
    {
        /// <summary>
        /// 默认 AUTH_NULL 0
        /// </summary>
        public int Flavor;
        public int Length;
    }

    /// <summary>
    /// 校验
    /// </summary>
    public class Verifier
    {
        /// <summary>
        /// 默认 AUTH_NULL 0
        /// </summary>
        public int Flavor;
        public int Length;
    }

    /// <summary>
    /// VXI11协议
    /// </summary>
    public class VXI11Protocol
    {
        /// <summary>
        /// 程序类型 默认VXI-11 Core 395183 0x000607af
        /// </summary>
        public int Program;

        /// <summary>
        /// 版本 默认1
        /// </summary>
        public int Version;

        /// <summary>
        /// 原型协议 默认tcp 6
        /// </summary>
        public int Proto;

        /// <summary>
        /// 端口 默认0
        /// </summary>
        public int Port;
    }
}
