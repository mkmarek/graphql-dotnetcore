namespace GraphQLCore.Utils
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class TypeUtilitites
    {
        public static object ParseBoolOrGiveNull(this string valueToParse)
        {
            bool outValue;

            if (bool.TryParse(valueToParse, out outValue))
                return outValue;

            return null;
        }

        public static object ParseFloatOrGiveNull(this string valueToParse)
        {
            float outValue;

            if (float.TryParse(valueToParse, NumberStyles.Float, CultureInfo.InvariantCulture, out outValue))
                return outValue;

            return null;
        }

        public static object ParseIntOrGiveNull(this string valueToParse)
        {
            int outValue;

            if (int.TryParse(valueToParse, NumberStyles.Integer, CultureInfo.InvariantCulture, out outValue))
                return outValue;

            return null;
        }

        public static object ParseLongOrGiveNull(this string valueToParse)
        {
            long outValue;

            if (long.TryParse(valueToParse, NumberStyles.Integer, CultureInfo.InvariantCulture, out outValue))
                return outValue;

            return null;
        }

        public static string ParseString(this object valueToParse)
        {
            if (valueToParse is bool)
                return valueToParse.ToString().ToLower();
            
            return Regex.Escape(valueToParse.ToString());
        }
    }
}