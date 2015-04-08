using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace Facepunch8.ViewModel
{
    [DataContract]
    public class BaseViewModel
    {
        public Color BgColor
        {
            get
            {
                if (App.IsLightTheme)
                    return Color.FromArgb(0xff, 0xc0, 0x1f, 0x25); //c01f25
                else
                    return Color.FromArgb(0xff, 0x22, 0x22, 0x22); //1f1f1f
            }
        }

        public Color FgColor
        {
            get
            {
                //if (App.IsLightTheme)
                return Color.FromArgb(0xff, 0xff, 0xff, 0xff); //c01f25
                /*else
                    return Color.FromArgb(0xff, 0, 0, 0); //1f1f1f*/
            }
        }

        public Color ThemeBgColor
        {
            get
            {
                if (App.IsLightTheme)
                    return Color.FromArgb(0xff, 0xff, 0xff, 0xff); //c01f25
                else
                    return Color.FromArgb(0xff, 0, 0, 0); //1f1f1f
            }
        }

        public SolidColorBrush ThemeBgBrush
        {
            get
            {
                return new SolidColorBrush(ThemeBgColor);
            }
        }

        public Color ThemeFgColor
        {
            get
            {
                var HeaderLight = (Settings.HeaderFooterTheme == Settings.Theme.Light || (Settings.HeaderFooterTheme == Settings.Theme.System && App.IsActualLightTheme));
                var Light = (Settings.CurrentTheme == Settings.Theme.Light || (Settings.CurrentTheme == Settings.Theme.System && App.IsActualLightTheme));
                if (
                    (HeaderLight && Light) ||
                    (!HeaderLight && Light) 
                    )
                    return Color.FromArgb(0xff, 0xff, 0xff, 0xff); //c01f25
                else
                    return Color.FromArgb(0xff, 0, 0, 0); //1f1f1
            }
        }

        public SolidColorBrush ThemeFgBrush
        {
            get
            {
                return new SolidColorBrush(ThemeFgColor);
            }
        }

        public SolidColorBrush BgBrush
        {
            get
            {
                return new SolidColorBrush(BgColor);
            }
        }

        public SolidColorBrush FgBrush
        {
            get
            {
                return new SolidColorBrush(FgColor);
            }
        }
    }
}
