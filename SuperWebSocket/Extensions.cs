using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket
{
    public static partial class Extensions
    {
        private readonly static char[] m_CrCf;

        static Extensions()
        {
            m_CrCf = "\r\n".ToArray();
        }

        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrCf);
        }

        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrCf);
        }

        public static void AppendWithCrCf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrCf);
        }

        public static void AppendWithCrCf(this StringBuilder builder)
        {
            builder.Append(m_CrCf);
        }

        private static Type[] m_SimpleTypes = new Type[] { 
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            };

        internal static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                m_SimpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}
