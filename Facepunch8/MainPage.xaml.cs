using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Facepunch8.Resources;
using Facepunch8.API;
using Facepunch8.Model;
using Facepunch8.ViewModel;

namespace Facepunch8
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static FPAPI api;
        private ForumViewModel _viewModel;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _viewModel = (ForumViewModel)Resources["viewModel"];
            this.DataContext = _viewModel;

            _viewModel.Load();
        }

        private void SubredditsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ForumsList.SelectedItem == null)
                return;

            ForumModel fm = (ForumModel)ForumsList.SelectedItem;

            ForumsList.SelectedItem = null;

            NavigationService.Navigate(new Uri("/Pages/ForumPage.xaml?forumid=" + fm.ForumID, UriKind.Relative));
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = _viewModel.BgColor;
            ApplicationBar.ForegroundColor = _viewModel.FgColor;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 0.8;

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton;
            if (!api.LoggedIn)
            {
                appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.key.png", UriKind.Relative));
                appBarButton.Text = "login";
                appBarButton.Click += LoginClick;
                ApplicationBar.Buttons.Add(appBarButton);
            }
            else
            {
                ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem("logout");
                appBarMenuItem.Click += LogoutClick;
                ApplicationBar.MenuItems.Add(appBarMenuItem);
            }
            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.cog.png", UriKind.Relative));
            appBarButton.Text = "settings";
            appBarButton.Click += SettingsButtonClick;
            ApplicationBar.Buttons.Add(appBarButton);

            // Create a new menu item with the localized string from AppResources.
            /*ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);*/
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back) return;

            string loggedin = ""; //May not be needed if you'll only ever go to page 2 from page 1 to download...
            if (NavigationContext.QueryString.TryGetValue("loggedin", out loggedin))
            {
                if (loggedin.ToLower().Equals("true"))
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;
                    settings.Add("sessionToken", api.SessionToken);
                    settings.Add("username", api.User.Name);
                    settings.Add("userid", api.User.UserID);
                    settings.Save();

                    //Don't want to be able to hit back and go back to the login screen...
                    while (this.NavigationService.BackStack.Any())
                    {
                        this.NavigationService.RemoveBackEntry();
                    }
                }
                if (loggedin.ToLower().Equals("false")) //logout
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;
                    settings.Remove("sessionToken");
                    settings.Remove("username");
                    settings.Remove("userid");
                    settings.Save();

                    while (this.NavigationService.BackStack.Any())
                    {
                        this.NavigationService.RemoveBackEntry();
                    }
                }
            }

            BuildLocalizedApplicationBar();

            SystemTray.BackgroundColor = _viewModel.BgColor;
            SystemTray.ForegroundColor = _viewModel.FgColor;
        }

        void LoginClick(object sender, EventArgs e)
        {
            if (api.LoggedIn)
                return;

            NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
        }

        void LogoutClick(object sender, EventArgs e)
        {
            if (!api.LoggedIn)
                return;

            api.Logout();
            NavigationService.Navigate(new Uri("/MainPage.xaml?loggedin=false", UriKind.Relative));
        }

        void SettingsButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
        }
    }
}