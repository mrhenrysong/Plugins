using System.Collections.Generic;
using System.Linq;

namespace Plugins.Base.ExtensionMethod.Pollutions
{
    /// <summary>
    /// string 的扩展方法包，用于给string类型附加扩展方法
    /// </summary>
    public static class StringPollution
    {
        /// <summary>
        /// 判断string类型是否为“”
        /// </summary>
        /// <param name="Source">值</param>
        /// <param name="NewSource">如果为“”则返回需要的值</param>
        /// <returns></returns>
        public static string DefaultValue(this string Source, string NewSource) => Source == "" ? NewSource : Source;

        /// <summary>
        /// 返回从一个符号到另一个符号中间的值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="start">起始符号</param>
        /// <param name="end">结束符号</param>
        /// <returns></returns>
        public static string GetInterspaceString(this string str, char start, char end)
        {
            int a = str.IndexOf(start);
            int b = str.LastIndexOf(end);
            string temp;
            try
            {
                temp = str.Substring(a + 1, b - a - 1);
            }
            catch { return ""; }
            return temp;
        }

        /// <summary>
        /// 字符串反转
        /// </summary>
        /// <param name="str">当前字符串</param>
        /// <returns>反转后的字符串</returns>
        public static string Evert(this string str)
        {
            string temp = string.Empty;
            int i = 0;
            while (i < str.Count())
            {
                IEnumerable<char> a = str.Skip(i).Take(8);
                foreach (char item in a.Reverse<char>())
                {
                    temp += item;
                }
                i += 8;
            }
            return temp;
        }

        /// <summary>
        /// 判断当前string是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmptyOrWhiteSpace(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 根据提供的总长度计算两个字符串的填充空间，确保它们居中，并且合并后字符串的总长度为指定的 totalLength
        /// </summary>
        /// <param name="str"></param>
        /// <param name="str2"></param>
        /// <param name="totalLength"></param>
        /// <returns></returns>
        public static string GetMergeStrings(this string str, string str2, int totalLength)
        {
            // 计算第一个字符串需要填充的空格数量
            int firstPadding = (totalLength - str.Length - 1) / 2;

            // 如果第一个字符串长度不够，则在右侧填充空格
            string paddedFirstString = str.PadRight(str.Length + firstPadding);

            // 计算第二个字符串需要填充的空格数量
            int secondPadding = totalLength - paddedFirstString.Length - str2.Length;

            // 如果第二个字符串长度不够，则在左侧填充空格，使其居中
            string paddedSecondString = str2.PadLeft(str2.Length + secondPadding);

            // 合并两个字符串
            string mergedString = paddedFirstString + ":" + paddedSecondString;

            return mergedString;
        }

    }
}
