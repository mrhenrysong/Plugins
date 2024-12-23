using System;

namespace Plugins.Communication.Extension.Helpers
{
    /// <summary>
    /// 枚举属性类
    /// 使用方法：USB <======> [EnumDescription("USB?")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumDescriptionAttribute : Attribute
    {
        private string description;
        public string Description { get { return description; } }

        public EnumDescriptionAttribute(string description)
            : base()
        {
            this.description = description;
        }
    }

    /// <summary>
    /// 枚举帮助类，利用枚举项打标签的方式来返回字符串，让枚举变量和字符串一一对应
    /// 使用方法：EnumHelper.GetDescription(Week.Monday)
    /// </summary>
    public static class EnumDescription
    {
        public static string GetDescription(System.Enum value)
        {
            if (value == null)
            {
                throw new ArgumentException("value");
            }
            string description = value.ToString();
            var fieldInfo = value.GetType().GetField(description);
            var attributes = (EnumDescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                description = attributes[0].Description;
            }
            return description;
        }
    }
}
