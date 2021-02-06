using System;
using System.Reflection;

namespace Pocosearch.Utils
{
    public static class AttributeExtensions
    {
        public static TAttribute GetCustomAttribute<TAttribute>(this Type t) where TAttribute : Attribute
        {
            return (TAttribute)Attribute.GetCustomAttribute(t, typeof(TAttribute));
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this PropertyInfo p) where TAttribute : Attribute
        {
            return (TAttribute)Attribute.GetCustomAttribute(p, typeof(TAttribute));
        }
    }
}