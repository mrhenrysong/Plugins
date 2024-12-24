namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// Int 的扩展方法包，用于给Int类型附加扩展方法
    /// </summary>
    public static class IntPollution
    {
        /// <summary>
        /// int转16进制string字符串
        /// </summary>
        /// <param name="a">需要转换的int数据</param>
        /// <returns></returns>
        public static string IntToString(this int a) => (0xff & a).ToString("X4");

        /// <summary>
        /// 将一个 int 类型的数值分解成两个 ushort（无符号短整型）值，并将它们作为数组返回
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static ushort[] GetArraryByInt(this int @this)
        {
            ushort[] result = new ushort[2];
            result[0] = (ushort)(@this & ushort.MaxValue);
            result[1] = (ushort)(@this >> 16);

            return result;
        }
    }
}
