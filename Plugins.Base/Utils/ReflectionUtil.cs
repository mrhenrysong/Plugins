using System;
using System.Reflection;

namespace Plugins.Base.Utils
{
    public class ReflectionUtil
    {
        public static void CopyProperties(object source, object target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            PropertyInfo[] outProperties = targetType.GetProperties();
            foreach (PropertyInfo outProperty in outProperties)
            {
                PropertyInfo inPorperty = sourceType.GetProperty(outProperty.Name);
                if (inPorperty != null && inPorperty.PropertyType == outProperty.PropertyType)
                {
                    outProperty.SetValue(target, inPorperty.GetValue(source));
                }
            }
        }
    }
}
