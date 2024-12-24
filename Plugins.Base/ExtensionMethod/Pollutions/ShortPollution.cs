using System;

namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// short 的扩展方法包，用于给short类型附加扩展方法
    /// </summary>
    public static class ShortPollution
    {
        /// <summary>
        /// 获取字节中的指定Bit的值
        /// </summary>
        /// <param name="this">字节</param>
        /// <param name="index">Bit的索引值(0-7)</param>
        /// <returns></returns>
        public static int GetBit(this ushort @this, short index)
        {
            return (@this & (1 << index)) == (1 << index) ? 1 : 0;
        }


        /// <summary>
        /// 以高字节在前的规则两个short合成一个int
        /// </summary>
        /// <param name="this"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int GetIntByShort(this ushort @this, int num)
        {
            return Convert.ToInt32((@this << 16) | num);
        }
    }

}
