using System;
using System.Collections.Generic;
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
using CaledosLab.Portable.Logging;

namespace Facepunch8.Pages
{
    public partial class ForumPage : PhoneApplicationPage
    {
        private ThreadViewModel _viewModel;
        private bool _isNewPageInstance = false;
        private int _forumId = -1;

        public ForumPage()
        {
            InitializeComponent();

            _isNewPageInstance = true;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // If this is a back navigation, the page will be discarded, so there
            // is no need to save state.
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                return;

            // Save the ViewModel variable in the page's State dictionary.
            State["ForumViewModel"] = _viewModel;
            State["API"] = MainPage.api;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                if (State.Count > 0)
                {
                    _viewModel = (ThreadViewModel)State["ForumViewModel"];
                    if (MainPage.api == null)
                        MainPage.api = (FPAPI)State["API"];
                }
                else
                {
                    _viewModel = new ThreadViewModel();

                    string forumid = ""; //May not be needed if you'll only ever go to page 2 from page 1 to download...
                    if (NavigationContext.QueryString.TryGetValue("forumid", out forumid))
                    {
                        if (Int32.TryParse(forumid, out _forumId))
                        {
                            Forum f = MainPage.api.GetForum(_forumId);
                            if (f != null)
                                _viewModel.Load(f, 1);
                        }
                    }
                }

                DataContext = _viewModel;
            }
            _isNewPageInstance = false;

            SystemTray.BackgroundColor = _viewModel.BgColor;
            SystemTray.ForegroundColor = _viewModel.FgColor;

            BuildLocalizedApplicationBar();

            /*if (e.NavigationMode == NavigationMode.Back) return;

            string forumid = ""; //May not be needed if you'll only ever go to page 2 from page 1 to download...
            if (NavigationContext.QueryString.TryGetValue("forumid", out forumid))
            {
                if (Int32.TryParse(forumid, out _forumId))
                {
                    Forum f = MainPage.api.GetForum(_forumId);
                    if (f != null)
                        _viewModel.Load(f, 1);
                }
            }*/
        }

        private void ThreadsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThreadsList.SelectedItem == null)
                return;

            if (this.postPopup.IsOpen)
            {
                this.postPopup.IsOpen = false;

                return;
            }

            ThreadModel t = (ThreadModel)ThreadsList.SelectedItem;

            ThreadsList.SelectedItem = null;
            NavigationService.Navigate(new Uri("/Pages/ThreadPage.xaml?threadid=" + t.ThreadID, UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = _viewModel.BgColor;
            ApplicationBar.ForegroundColor = _viewModel.FgColor;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 0.8;

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.arrow.left.png", UriKind.Relative));
            appBarButton.Text = "previous";
            appBarButton.Click += PreviousPageClick;
            ApplicationBar.Buttons.Add(appBarButton);

            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.refresh.png", UriKind.Relative))
            {
                Text = "refresh"
            };
            appBarButton.Click += RefreshClick;
            ApplicationBar.Buttons.Add(appBarButton);

            if (MainPage.api.LoggedIn)
            {
                appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.message.png", UriKind.Relative))
                {
                    Text = "post"
                };
                appBarButton.Click += OpenPostThreadClick;
                ApplicationBar.Buttons.Add(appBarButton);
            }

            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.arrow.right.png", UriKind.Relative))
            {
                Text = "next"
            };
            appBarButton.Click += NextPageClick;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.postPopup.IsOpen)
            {
                this.postPopup.IsOpen = false;

                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        void PreviousPageClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.PREVIOUS);
        }

        void NextPageClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.NEXT);
        }

        void RefreshClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.REFRESH);
        }

        void OpenPostThreadClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen)
                return;

            //Reset stuffz.
            this.threadContent.Text = "";
            this.threadTitle.Text = "";
            this.postPopup.IsOpen = true;
        }

        void PostThreadClick(object sender, EventArgs e)
        {
            if (threadContent.Text == "" || threadTitle.Text == "")
            {
                MessageBox.Show("Please enter a title and a message!");
            }
            else
            {
                this.postPopup.IsOpen = false;

                //TODO: status
                //Looks odd, but it's so I don't mess up existing \r\n's.
                MainPage.api.PostThread(_forumId, threadTitle.Text.Replace("\r\n", "\r").Replace("\r", "\r\n"), threadContent.Text, msg =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _viewModel.ChangePage(PageDirection.REFRESH);
                    });
                }, (err, ex) =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Error occurred while attempting to post.");
                        Logger.WriteLine(ex);
                    });
                });
            }
        }
    }
}