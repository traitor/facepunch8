using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Facepunch8.Pages
{
    public partial class LoginPage : PhoneApplicationPage
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

        public LoginPage()
        {
            InitializeComponent();

            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.BackgroundColor = BgColor;
            SystemTray.ForegroundColor = FgColor;
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            if (!usernameTb.Text.Contains("@"))
            {
                MessageBox.Show(
                    "Due to API limitations, you must enter the email address associated with your Facepunch account.");
                return;
            }

            var api = MainPage.api;

            api.Login(usernameTb.Text, passwordTb.Password, user =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        NavigationService.Navigate(new Uri("/MainPage.xaml?loggedin=true", UriKind.Relative));
                    });
            }, (err, ex) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(err);
                    });
            });
        }

        private void usernameTb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}