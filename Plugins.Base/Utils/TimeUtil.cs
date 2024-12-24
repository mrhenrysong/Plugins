using System;
using System.Runtime.InteropServices;

namespace Plugins.Base.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }

    public class TimeUtil
    {
        /// <summary>
        /// 时间戳计时开始时间
        /// </summary>
        private static DateTime timeStampStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long TimeStamp()
        {
            return (long)(DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalSeconds;
        }

        /// <summary>
        /// DateTime转换为10位时间戳（单位：秒）
        /// </summary>
        /// <param name="dateTime"> DateTime</param>
        /// <returns>10位时间戳（单位：秒）</returns>
        public static long DateTimeToTimeStamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - timeStampStartTime).TotalSeconds;
        }

        /// <summary>
        /// DateTime转换为13位时间戳（单位：毫秒）
        /// </summary>
        /// <param name="dateTime"> DateTime</param>
        /// <returns>13位时间戳（单位：毫秒）</returns>
        public static long DateTimeToLongTimeStamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - timeStampStartTime).TotalMilliseconds;
        }

        /// <summary>
        /// 10位时间戳（单位：秒）转换为DateTime
        /// </summary>
        /// <param name="timeStamp">10位时间戳（单位：秒）</param>
        /// <returns>DateTime</returns>
        public static DateTime TimeStampToDateTime(long timeStamp)
        {
            return timeStampStartTime.AddSeconds(timeStamp).ToLocalTime();
        }

        /// <summary>
        /// 13位时间戳（单位：毫秒）转换为DateTime
        /// </summary>
        /// <param name="longTimeStamp">13位时间戳（单位：毫秒）</param>
        /// <returns>DateTime</returns>
        public static DateTime LongTimeStampToDateTime(long longTimeStamp)
        {
            return timeStampStartTime.AddMilliseconds(longTimeStamp).ToLocalTime();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetLocalTime(ref SystemTime time);

        /// <summary>
        /// 设置本地系统时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool SetLocalTime(DateTime time)
        {
            SystemTime st = new SystemTime
            {
                Year = Convert.ToInt16(time.Year),
                Month = Convert.ToInt16(time.Month),
                Day = Convert.ToInt16(time.Day),
                DayOfWeek = Convert.ToInt16(time.DayOfWeek),
                Hour = Convert.ToInt16(time.Hour),
                Minute = Convert.ToInt16(time.Minute),
                Second = Convert.ToInt16(time.Second),
                Milliseconds = Convert.ToInt16(time.Millisecond)
            };
            return SetLocalTime(ref st);
        }
    }
}
