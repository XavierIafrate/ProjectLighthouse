using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class ValidationHelper
    {
        private static Regex AlphanumericAndSpaces = new(@"^[a-zA-Z0-9\s]*$", RegexOptions.Compiled);

        private static Regex Alphanumeric = new(@"^[a-zA-Z0-9]*$", RegexOptions.Compiled);
        private static Regex UpperCaseAndNumbers = new(@"^[A-Z0-9]*$", RegexOptions.Compiled);
        private static Regex Numbers = new(@"^[0-9]*$", RegexOptions.Compiled);

        private static Regex ProductName = new(@"^[A-Z0-9.-]*$", RegexOptions.Compiled);
        private static Regex ProductNameChars = new(@"[A-Z0-9.-]", RegexOptions.Compiled);

        public static bool StringIsNumbers(string str)
        {
            if (str is null) return false;
            Regex rx = Numbers;

            return rx.IsMatch(str);
        }

        public static bool StringIsUpperCaseAndNumbers(string str)
        {
            Regex rx = UpperCaseAndNumbers;

            return rx.IsMatch(str);
        }

        public static bool StringIsAlphanumeric(string str, bool allowSpace = false)
        {
            Regex rx = allowSpace
                ? AlphanumericAndSpaces
                : Alphanumeric;

            return rx.IsMatch(str);
        }

        public static bool IsValidProductName(string productName)
        {
            Regex rx = ProductName;
            return rx.IsMatch(productName);
        }

        public static string GetInvalidProductNameChars(string productName)
        {
            return GetNonMatchingCharacters(ProductNameChars, productName);
        }

        public static string MakeValidFileName(string input)
        {
            input = input.Trim().ToUpperInvariant();
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(input, invalidRegStr, "_");
        }

        public static string GetNonMatchingCharacters(Regex rx, string input)
        {
            char[] chars = input.ToCharArray().Distinct().ToArray();

            string result = "";
            for (int i = 0; i < chars.Length; i++)
            {
                if (!rx.IsMatch(chars[i].ToString()))
                {
                    result += result == ""
                        ? $"'{chars[i]}'"
                        : $", '{chars[i]}'";
                }
            }

            return result;
        }
    }
}
