using Plugins.Communication.Extension.Helpers;
using Plugins.Communication.VISA.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins.Communication.VISA
{
    public class DriveIO : IDisposable
    {
        private int vi = 0;
        private readonly int rm = 0;
        private short intfType = 0;
        private short intfNum = 0;
        private const short WaitTimeout = 2000;
        //线程互斥读写锁
        private static readonly object locker = new object();

        /// <summary>
        /// 创建一个默认的资源管理器后打开address的设备 超时时间2s
        /// </summary>
        /// <param name="address">设备visa地址</param>
        /// <exception cref="Exception"></exception>
        public DriveIO(string address)
        {

            int status = Visa32.viOpenDefaultRM(out rm);
            if (status != Visa32.VI_SUCCESS)
            {
                throw new Exception($"viOpenDefaultRM Fail Exception:{status}");
                // return;

            }

            status = Visa32.viParseRsrc(rm, address, ref intfType, ref intfNum);
            if (status != Visa32.VI_SUCCESS)
            {
                throw new Exception($"viParseRsrc Fail Exception:{status}");
            }

            status = Visa32.viOpen(rm, address, 0, WaitTimeout, out vi);
            if (status != Visa32.VI_SUCCESS)
            {
                throw new Exception($"viOpen Fail Exception:{status}");
                // return;
            }
            SetTERMCHAR();

        }
        /// <summary>
        /// 创建一个默认的资源管理器后打开address的设备 超时时间waitForLockTimeout
        /// </summary>
        /// <param name="Address">设备visa地址</param>
        /// <param name="waitForLockTimeout">超时时间</param>
        /// <exception cref="Exception"></exception>
        public DriveIO(string Address, int waitForLockTimeout)
        {

            int status = Visa32.viOpenDefaultRM(out rm);
            if (status != 0)
            {
                throw new Exception($"viOpenDefaultRM Fail Exception:{status}");

            }

            status = Visa32.viParseRsrc(rm, Address, ref intfType, ref intfNum);
            if (status != 0)
            {
                throw new Exception($"viParseRsrc Fail Exception:{status}");
            }

            status = Visa32.viOpen(rm, Address, 0, waitForLockTimeout, out vi);
            if (status != 0)
            {
                throw new Exception($"viOpen Fail Exception:{status}");
            }
            SetTERMCHAR();

        }

        /// <summary>
        /// 只创建一个默认的资源管理器
        /// </summary>
        /// <exception cref="Exception"></exception>
        public DriveIO()
        {
            int status = Visa32.viOpenDefaultRM(out rm);
            if (status != 0)
            {
                throw new Exception("viOpenDefaultRM Fail");
            }
        }

        /// <summary>
        /// 以visa地址打开资源 超时时间2s 调用此接口必须申明rm
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool OpenResource(string address)
        {
            if (rm != 0)
            {
                int status = Visa32.viParseRsrc(rm, address, ref intfType, ref intfNum);
                if (status == 0)
                {
                    status = Visa32.viOpen(rm, address, 0, WaitTimeout, out vi);
                    if (status != 0)
                    {
                        return false;
                    }
                }

                SetTERMCHAR();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 查找资源
        /// </summary>
        /// <param name="filter">正则表达式过滤器</param>

        public List<string> FindResources(FilterDescriptionType type)
        {
            //通过枚举变量值获得描述字符串
            string filter = EnumDescription.GetDescription(type);

            List<string> address = new List<string>();

            //所有返回的字符串都需要经过viParseRsrc与viOpen，不是所有的查询到的资源都可以通过的
            StringBuilder desc = new StringBuilder();
            Visa32.viFindRsrc(rm, filter, out int rsrcVi, out int count, desc);

            address.Add(desc.ToString());

            if (count > 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    Visa32.viFindNext(rsrcVi, desc);
                    address.Add(desc.ToString());
                }
            }

            return address;
        }

        private void SetTERMCHAR()
        {
            Visa32.viSetAttribute(vi, Visa32.VI_ATTR_TERMCHAR, 10);
            Visa32.viSetAttribute(vi, Visa32.VI_ATTR_TERMCHAR_EN, Visa32.VI_TRUE);
        }

        public int Timeout
        {
            set
            {

                Visa32.viSetAttribute(vi, Visa32.VI_ATTR_TMO_VALUE, value);
            }
            get
            {
                Visa32.viGetAttribute(vi, Visa32.VI_ATTR_TMO_VALUE, out int iResultValue);
                return iResultValue;
            }

        }

        public void WriteLine(string writeFmt)
        {
            lock (locker)
            {
                if (!writeFmt.Contains("\n"))
                    writeFmt += "\n";
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(writeFmt);
                Visa32.viWrite(vi, byteArray, byteArray.Length, out int iOutResult);
                //  Visa32Util.viPrintf(vi, writeFmt);
            }

        }

        public void WriteLine(byte[] writeFmt)
        {
            lock (locker)
            {
                Visa32.viWrite(vi, writeFmt, writeFmt.Length, out int iOutResult);
            }

        }

        public void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            lock (locker)
            {
                object[] args = new object[] { arg0, arg1, arg2 };
                this.WriteLine(format, args);
            }

        }

        public void WriteLine(string format, params object[] args)
        {
            lock (locker)
            {
                string text = string.Format(format, args);
                WriteLine(text);
            }

        }

        public string Read()
        {
            lock (locker)
            {
                Visa32.viRead(vi, out string RetrunStr, 1024);

                return RetrunStr.TrimEnd('\n').Trim('\"');
            }


        }

        public string ReadResult()
        {
            lock (locker)
            {
                int iCount = 1000;
                byte[] bBuffer = new byte[iCount];
                int iResult;
                string sResult = "";
                do
                {
                    iResult = Visa32.viRead(vi, bBuffer, iCount, out int iRetCount);
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    string asciiResults = encoding.GetString(bBuffer, 0, iRetCount);
                    sResult += asciiResults;

                    //} while (iResult >= Visa32.VI_SUCCESS && iResult != Visa32.VI_SUCCESS_TERM_CHAR);
                } while (iResult > Visa32.VI_SUCCESS && iResult != Visa32.VI_SUCCESS_TERM_CHAR);
                return sResult.ToString();
            }
        }

        /// <summary>
        /// 获取数据后转成double数组 封装供给频谱仪获取迹线数据使用
        /// </summary>
        /// <returns></returns>
        public double[] ReadToDoubleArray()
        {
            string Temp = ReadResult();
            Temp = Temp.TrimEnd('\n').Trim('\"');
            var vTemp = Temp.Split(',');
            int iLength = vTemp.Length;
            double[] retDouble = new double[iLength];
            for (int i = 0; i < iLength; i++)
            {
                retDouble[i] = double.Parse(vTemp[i]);
            }
            return retDouble;
        }

        public string Query(string writeFmt)
        {
            lock (locker)
            {
                if (!writeFmt.Contains("\n"))
                    writeFmt += "\n";
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(writeFmt);
                Visa32.viWrite(vi, byteArray, byteArray.Length, out int iOutResult);

                return ReadResult();
            }

        }

        public int Close()
        {
            Visa32.viClose(vi);
            return Visa32.viClose(rm);
        }

        public int OnlyCloseRM()
        {
            return Visa32.viClose(rm);
        }

        public int OnlyCloseVI()
        {
            return Visa32.viClose(vi);
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }
    }
}
