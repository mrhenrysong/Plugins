using System.Configuration;
using System.Linq;

namespace Plugins.Base.Helpers
{
    public class LocalConfigHelper
    {
        /// <summary>
        /// 获取指定的key对应的内容
        /// </summary>
        /// <param name="key">要查询的配置key</param>
        /// <returns>返回配置中对应的内容</returns>
        public static string GetAppSettingValue(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];
            return null;
        }

        /// <summary>
        /// 写入新的配置信息
        /// </summary>
        /// <param name="key">配置key</param>
        /// <param name="data">配置信息</param>
        public static void SetAppSettingValue(string key, string data)
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings[key].Value = data;
            cfa.Save();
        }
    }
}
