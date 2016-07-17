namespace GraphQLCore.Utils
{
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

            if (float.TryParse(valueToParse, out outValue))
                return outValue;

            return null;
        }

        public static object ParseIntOrGiveNull(this string valueToParse)
        {
            int outValue;

            if (int.TryParse(valueToParse, out outValue))
                return outValue;

            return null;
        }
    }
}