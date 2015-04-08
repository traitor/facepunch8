using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8
{
    class Settings
    {
        public enum Theme { System, Light, Dark };
        private static bool _displayImages = false;
        private static Theme _theme = Theme.System;
        private static Theme _headerTheme = Theme.System;

        public static bool DisplayImages
        {
            get
            {
                return _displayImages;
            }
            set
            {
                _displayImages = value;
                Save("displayImages", value);
            }
        }

        public static Theme CurrentTheme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
                Save("currentTheme", value);
            }
        }

        public static Theme HeaderFooterTheme
        {
            get
            {
                return _headerTheme;
            }
            set
            {
                _headerTheme = value;
                Save("currentHeaderTheme", value);
            }
        }

        public static void Initialize()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("displayImages"))
                _displayImages = (bool)settings["displayImages"];
            else
                DisplayImages = false;

            //Content theme
            if (settings.Contains("currentTheme"))
                _theme = (Theme)settings["currentTheme"];
            else
                CurrentTheme = Theme.System;

            //Header/footer theme
            if (settings.Contains("currentHeaderTheme"))
                _headerTheme = (Theme)settings["currentHeaderTheme"];
            else
                HeaderFooterTheme = Theme.System;
        }

        private static void Save(string name, object value)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(name))
                settings[name] = value;
            else
                settings.Add(name, value);

            settings.Save();
        }

        private static T Load<T>(string name)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(name))
                return (T) settings[name];
            else
                return default(T);
        }
    }
}
