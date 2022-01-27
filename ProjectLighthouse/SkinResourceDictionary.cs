using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse
{
    public class SkinResourceDictionary : ResourceDictionary
    {
        private Uri _classicSource;
        private Uri _darkSource;

        public Uri ClassicSource
        {
            get { return _classicSource; }
            set
            {
                _classicSource = value;
                UpdateSource();
            }
        }
        public Uri DarkSource
        {
            get { return _darkSource; }
            set
            {
                _darkSource = value;
                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            var val = App.Skin == Skin.Classic ? ClassicSource : DarkSource;
            if (val != null && base.Source != val)
            {
                base.Source = val;
            }
        }
    }
}
