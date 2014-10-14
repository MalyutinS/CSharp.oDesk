using System;
using System.Globalization;
using Spring.Json;

namespace CSharp.oDesk.JobsSearch.Helpers
{
    public static class JsonValueHelper
    {
        public static double ToDouble(this JsonValue value)
        {
            if (value == null || value.IsNull)
            {
                return 0;
            }
            return Convert.ToDouble(value.ToStringWithoutQuotes(), CultureInfo.InvariantCulture);
        }

        public static int ToInt32(this JsonValue value)
        {
            if (value == null || value.IsNull)
            {
                return 0;
            }
            return Convert.ToInt32(value.ToStringWithoutQuotes(), CultureInfo.InvariantCulture);
        }

        public static DateTime ToDateTime(this JsonValue value)
        {
            if (value == null || value.IsNull || string.IsNullOrEmpty(ToStringWithoutQuotes(value)))
            {
                return new DateTime(2000, 1, 1);
            }
            return Convert.ToDateTime(value.ToStringWithoutQuotes(), CultureInfo.InvariantCulture);
        }

        public static string ToStringWithoutQuotes(this JsonValue value)
        {
            if (value == null || value.IsNull)
            {
                return string.Empty;
            }
            return value.ToString().Replace("\"", "");
        }
    }
}
