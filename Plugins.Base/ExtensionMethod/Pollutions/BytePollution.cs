using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// Byte 的扩展方法包，用于给Byte类型附加扩展方法
    /// </summary>
    public static class BytePollution
    {
        /// <summary>
        /// 把Byte数组连接成FF FF FF格式的字符串
        /// </summary>
        /// <param name="by">Byte数组</param>
        /// <returns>连接后的字符串</returns>
        public static string GetString(this byte[] by)
        {
            StringBuilder sb = new StringBuilder();
            by.ForEach(i => sb.Append(i.ToString("x2") + " "));
            return sb.ToString();
        }

        /// <summary>
        /// 把Byte数组按设定的数量分割成小数组用于传输
        /// </summary>
        /// <param name="by">原始数组</param>
        /// <param name="number">设定分割数组用的数量</param>
        /// <returns>返回小数组的List</returns>
        public static List<byte[]> GetSegmentation(this byte[] by, int number)
        {
            double ceiling = Math.Ceiling((double)by.Length / number);
            List<byte[]> list = new List<byte[]>();
            for (int i = 0; i < ceiling; i++)
            {
                byte[] s = by.Skip(i * number).Take(number).ToArray();
                list.Add(s);
            }
            return list;
        }

        /// <summary>
        /// 反转当前byte数组
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public static byte[] GetEvert(this byte[] by)
        {
            List<byte> by1 = new List<byte>();
            for (int i = by.Length - 1; i >= 0; i--)
            {
                by1.Add(by[i]);
            }
            return by1.ToArray();
        }

        /// <summary>
        /// 将一个 byte[] 数组转换为一个十六进制表示的字符串
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] by)
        {
            string HexString = string.Empty;
            if (by != null)
            {
                StringBuilder strB = new StringBuilder();
                foreach (byte item in by)
                {
                    strB.Append(item.ToString("X2"));
                }
                HexString = strB.ToString();
            }
            return HexString;
        }

    }
}
