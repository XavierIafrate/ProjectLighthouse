
using System.Collections.Generic;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class TextBoxHelper
    {
        public static bool ValidateKeyPressNumbersOnly(KeyEventArgs e)
        {
            string strKey = e.Key.ToString();

            if ((strKey.Contains("D") && strKey.Length == 2) || strKey.Contains("NumPad"))
                if ("0123456789".Contains(strKey.Substring(strKey.Length - 1, 1)))
                    return false;

            List<string> nonNumericAllowedKeys = new()
            {
                "Back",
                "Left",
                "Right",
                "Up",
                "Down",
                "Delete"
            };

            if (nonNumericAllowedKeys.Contains(strKey))
                return false;

            return true;
        }

        public static bool ValidateKeyPressNumbersAndPeriod(string existingText, KeyEventArgs e)
        {
            string strKey = e.Key.ToString();

            if ((strKey.Contains("D") && strKey.Length == 2) || strKey.Contains("NumPad"))
                if ("0123456789".Contains(strKey.Substring(strKey.Length - 1, 1)))
                    return false;

            List<string> nonNumericAllowedKeys = new()
            {
                "Back",
                "Left",
                "Right",
                "Up",
                "Down",
                "Delete",
            };

            if (nonNumericAllowedKeys.Contains(strKey))
                return false;

            if (strKey is "Decimal" or "OemPeriod")
            {
                if (!existingText.Contains("."))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateAlphanumeric(KeyEventArgs e)
        {
            string strKey = e.Key.ToString();

            if ((strKey.Contains("D") && strKey.Length == 2) || strKey.Contains("NumPad"))
                if ("0123456789".Contains(strKey.Substring(strKey.Length - 1, 1)))
                    return false;

            if (strKey.Length == 1)
                if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(strKey))
                    return false;

            List<string> nonNumericAllowedKeys = new()
            {
                "Back",
                "Left",
                "Right",
                "Up",
                "Down",
                "Delete",
            };

            if (nonNumericAllowedKeys.Contains(strKey))
                return false;

            return true;
        }
    }
}
