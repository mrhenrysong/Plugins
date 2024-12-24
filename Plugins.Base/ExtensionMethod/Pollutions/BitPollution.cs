using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// Bit 的扩展方法包，用于给Bit类型附加扩展方法
    /// </summary>
    public static class BitPollution
    {
        public delegate string charater(BitArray bit);

        /// <summary>
        /// 位转byte
        /// </summary>
        /// <param name="bits">位数组</param>
        /// <returns></returns>
        public static byte ConvertToByte(this BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        /// <summary>
        /// 污染委托,字符处理
        /// </summary>
        /// <param name="bit">bit</param>
        /// <param name="charater">接受一个bit,返回string</param>
        public static string Charater(this BitArray bit, charater charater) => charater(bit);

        /// <summary>
        /// 转换位为字符串
        /// </summary>
        /// <param name="bit">位</param>
        /// <returns></returns>
        public static string GetString(this BitArray bit)
        {
            string str = null;
            foreach (object item in bit)
            {
                str += Convert.ToByte(item).ToString();
            }
            return str;
        }

        /// <summary>
        /// 对位进行处理,返回string
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static string BitAter(this BitArray bit, int Begin, int Ending, string name, string State1, string State2, Func<string, bool> func)
        {
            string StrBT = bit.GetString().Substring(Begin, Ending);
            return func(StrBT) ? name + ":" + State1 : name + ":" + State2;
        }

        /// <summary>
        /// 把位转换为int
        /// </summary>
        /// <param name="bit">位</param>
        /// <returns></returns>
        public static int GetInt(this BitArray bit)
        {
            string strBT = bit.GetString();
            return Convert.ToInt32(strBT, 2);
        }

        /// <summary>
        /// 反转当前bit数据
        /// </summary>
        /// <param name="bit">位</param>
        /// <returns></returns>
        public static void Evert(this BitArray bit, int w)
        {

            int a = w;
            int i = 0;
            List<bool[]> list = new List<bool[]>();
            while (a != bit.Length + w)
            {
                IEnumerable<bool> ie = bit.OfType<bool>().Skip(bit.Length - a).Take(w);
                list.Add(ie.ToArray());
                a += w;
            }
            foreach (bool[] item in list)
            {
                foreach (bool items in item)
                {
                    bit[i] = items;
                    i++;
                }

            }


        }

        /// <summary>
        /// 反转当前bit数据
        /// </summary>
        /// <param name="bit">位</param>
        /// <returns></returns>
        public static void Evert(this BitArray bit)
        {
            IEnumerable<bool> ie = bit.OfType<bool>();
            bool[] Bl = ie.ToArray<bool>().Reverse<bool>().ToArray<bool>();
            int i = 0;
            foreach (bool item in Bl)
            {
                bit[i] = item;
                i++;
            }
        }
    }
}
