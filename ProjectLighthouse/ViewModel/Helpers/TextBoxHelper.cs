
using System.Diagnostics;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class TextBoxHelper
    {
        public static bool ValidateKeyPressNumbersOnly(KeyEventArgs e)
        {
            Debug.WriteLine(string.Format("Key pressed: {0}", e.Key.ToString()));
            if (e.Key == Key.Space)
                return true;

            string strKey = e.Key.ToString();

            if ((strKey.Contains("D") && strKey.Length == 2) || strKey.Contains("NumPad"))
                if ("0123456789".Contains(strKey.Substring(strKey.Length - 1, 1)))
                    return false;
            
            if ("BackDeleteLeftRightUpDown".Contains(strKey))
                return false;

            return true;
        }
    }
}
