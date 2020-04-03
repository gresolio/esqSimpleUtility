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

            if (alphanumRegex.IsMatch(str))
                return true;
            else
                return false;
        }

        public static uint CountLeadingSpaces(this string str)
        {
            uint count = 0;

            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == ' ')
                        count++;
                    else
                        break;
                }
            }

            return count;
        }

        public static uint CountTrailingSpaces(this string str)
        {
            uint count = 0;

            if (!string.IsNullOrEmpty(str))
            {
                for (int i = str.Length - 1; i >= 0; i--)
                {
                    if (str[i] == ' ')
                        count++;
                    else
                        break;
                }
            }

            return count;
        }
    }
}
