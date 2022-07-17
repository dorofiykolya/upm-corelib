namespace Framework.Runtime.Utilities
{
    public static class NumberConversionUtils
    {
        private const string MillionFormat = "M";
        private const string ThousandFormat = "k";

        private const int MillionDeterminant = 999999;
        private const int ThousandDeterminant = 999;

        public static string ToHumanReadable(this long value)
        {
            return Conversion(value);
        }
        
        public static string ToHumanReadable(this int value)
        {
            return Conversion(value);
        }

        public static string Conversion(long value)
        {
            string result;

            if (value > MillionDeterminant)
            {
                result = (value / 1000000).ToString("0") + MillionFormat;
                return result;
            }

            if (value > ThousandDeterminant)
            {
                result = (value / 1000).ToString("0") + ThousandFormat;
                return result;
            }

            result = value.ToString();
            return result;
        }

        public static string Conversion(string value)
        {
            long number = long.Parse(value);
            return Conversion(number);
        }
    }
}