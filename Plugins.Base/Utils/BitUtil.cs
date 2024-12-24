using System;

namespace Plugins.Base.Utils
{
    public class BitUtil
    {
        /// <summary>
        /// 获取一个字节某一位的值是否为1
        /// </summary>
        /// <param name="value">给定字节</param>
        /// <param name="index">要获取的位 范围0-7</param>
        /// <returns>该字节指定位的值是否为1</returns>
        public static bool GetBit(byte value, int index)
        {
            if (index < 0 || index > 7)
            {
                throw new ArgumentOutOfRangeException();
            }
            return ((value >> index) & 1) == 1;
        }

        /// <summary>
        /// 设置某一位的值
        /// </summary>
        /// <param name="data">需要设置的byte数据</param>
        /// <param name="index">要设置的位， 值从低到高为 1-8</param>
        /// <param name="flag">要设置的值 true / false</param>
        /// <returns></returns>
        public static byte SetBit(byte data, int index, bool flag)
        {
            index++;

            if (index > 8 || index < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }

    }
}
