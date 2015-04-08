using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Facepunch8.ViewModel;

namespace Facepunch8.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        SettingViewModel _vm;

        public SettingsPage()
        {
            InitializeComponent();
            _vm = new SettingViewModel();

            DataContext = _vm;

            Loaded += (s, e) =>
            {
                if (Settings.DisplayImages)
                    imagePicker.SelectedIndex = 1;

                if (Settings.CurrentTheme == Settings.Theme.Dark)
                    themePicker.SelectedIndex = 1;
                else if (Settings.CurrentTheme == Settings.Theme.Light)
                    themePicker.SelectedIndex = 2;

                if (Settings.HeaderFooterTheme == Settings.Theme.Dark)
                    headerThemePicker.SelectedIndex = 1;
                else if (Settings.HeaderFooterTheme == Settings.Theme.Light)
                    headerThemePicker.SelectedIndex = 2;
            };

            BuildLocalizedApplicationBar();
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = _vm.BgColor;
            ApplicationBar.ForegroundColor = _vm.FgColor;

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.save.png", UriKind.Relative));
            appBarButton.Text = "save";
            appBarButton.Click += ApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (imagePicker.SelectedIndex == 0)
            {
                Settings.DisplayImages = false;
            }
            else
            {
                Settings.DisplayImages = true;
            }

            switch (themePicker.SelectedIndex)
            {
                case 0: Settings.CurrentTheme = Settings.Theme.System; break;
                case 1: Settings.CurrentTheme = Settings.Theme.Dark; break;
                case 2: Settings.CurrentTheme = Settings.Theme.Light; break;
            }

            switch (headerThemePicker.SelectedIndex)
            {
                case 0: Settings.HeaderFooterTheme = Settings.Theme.System; break;
                case 1: Settings.HeaderFooterTheme = Settings.Theme.Dark; break;
                case 2: Settings.HeaderFooterTheme = Settings.Theme.Light; break;
            }

            NavigationService.GoBack();
        }

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void themePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}