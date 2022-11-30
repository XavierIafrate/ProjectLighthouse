using System.IO;
using System.Text.RegularExpressions;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class ValidationHelper
    {
        private static Regex AlphanumericAndSpaces = new(@"^[a-zA-Z0-9\s]*$", RegexOptions.Compiled);
        private static Regex Alphanumeric = new(@"^[a-zA-Z0-9]*$", RegexOptions.Compiled);
        private static Regex ProductName = new(@"^[A-Z0-9.-]*$", RegexOptions.Compiled);
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

        public static string MakeValidFileName(string input)
        {
            input = input.Trim().ToUpperInvariant();
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(input, invalidRegStr, "_");
        }
    }
}
