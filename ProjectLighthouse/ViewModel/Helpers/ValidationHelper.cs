using System.Text.RegularExpressions;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class ValidationHelper
    {
        private static Regex AlphanumericAndSpaces = new(@"^[a-zA-Z0-9\s]*$", RegexOptions.Compiled);
        private static Regex Alphanumeric = new(@"^[a-zA-Z0-9]*$", RegexOptions.Compiled);
        public static bool StringIsAlphanumeric(string str, bool allowSpace = false)
        {
            Regex rx = allowSpace
                ? AlphanumericAndSpaces
                : Alphanumeric;

            return rx.IsMatch(str);
        }
    }
}
