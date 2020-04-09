using System.Linq;
using System.Text.RegularExpressions;

namespace esqSimpleUtility.Tools
{
    public static class StringExtensions
    {
        private static readonly Regex alphanumRegex = new Regex("^[a-zA-Z0-9]*$");
        public static bool IsAlphanumeric(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return alphanumRegex.IsMatch(str);
        }

        public static int CountLeadingSpaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            return str.TakeWhile(c => c == ' ').Count();
        }

        public static int CountTrailingSpaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            return str.Reverse().TakeWhile(c => c == ' ').Count();
        }
    }
}
