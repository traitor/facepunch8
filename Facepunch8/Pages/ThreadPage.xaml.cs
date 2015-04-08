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
using CodeKicker.BBCode;
using CaledosLab.Portable.Logging;

namespace Facepunch8.Pages
{
    public partial class ThreadPage : PhoneApplicationPage
    {
        private PostViewModel _viewModel;
        private int _threadId;
        private bool _isNewPageInstance = false;

        public ThreadPage()
        {
            InitializeComponent();

            _viewModel = (PostViewModel)Resources["viewModel"];
            _isNewPageInstance = true;
            DataContext = _viewModel;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // If this is a back navigation, the page will be discarded, so there
            // is no need to save state.
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                // Save the ViewModel variable in the page's State dictionary.
                State["PostViewModel"] = _viewModel;
                State["API"] = MainPage.api;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                if (State.Count > 0)
                {
                    _viewModel = (PostViewModel)State["PostViewModel"];
                    MainPage.api = (FPAPI)State["API"];
                }
                else
                {
                    _viewModel = new PostViewModel();

                    string threadidstr = ""; //May not be needed if you'll only ever go to page 2 from page 1 to download...
                    if (NavigationContext.QueryString.TryGetValue("threadid", out threadidstr))
                    {
                        int threadid = -1;
                        if (Int32.TryParse(threadidstr, out threadid))
                        {
                            _threadId = threadid;
                            _viewModel.Load(threadid, 1);
                        }
                    }
                }

                DataContext = _viewModel;
            }

            SystemTray.BackgroundColor = _viewModel.BgColor;
            SystemTray.ForegroundColor = _viewModel.FgColor;

            BuildLocalizedApplicationBar();
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

            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.refresh.png", UriKind.Relative));
            appBarButton.Text = "refresh";
            appBarButton.Click += RefreshClick;
            ApplicationBar.Buttons.Add(appBarButton);

            if (MainPage.api.LoggedIn)
            {
                appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.message.png", UriKind.Relative));
                appBarButton.Text = "post";
                appBarButton.Click += PostClick;
                ApplicationBar.Buttons.Add(appBarButton);
            }

            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/appbar.arrow.right.png", UriKind.Relative));
            appBarButton.Text = "next";
            appBarButton.Click += NextPageClick;
            ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem("jump to");
            appBarMenuItem.Click += ShowJumpPopup;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.jumpToPopup.IsOpen || this.postPopup.IsOpen)
            {
                this.jumpToPopup.IsOpen = false;
                this.postPopup.IsOpen = false;
                //TODO: empty message content maybe?
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }


        private void ShowJumpPopup(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            this.jumpToPopup.IsOpen = true;
            pageNumberTb.Focus();
        }

        void PreviousPageClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.PREVIOUS);
        }

        void NextPageClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.NEXT);
        }

        void RefreshClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            _viewModel.ChangePage(PageDirection.REFRESH);
        }

        void PostClick(object sender, EventArgs e)
        {
            if (this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            this.postContent.Text = ""; //Reset content
            this.postPopup.IsOpen = true;
        }

        void Image_Click(object sender, EventArgs e)
        {
            MessageBox.Show("yay");
        }

        void PostMessageClick(object sender, EventArgs e)
        {
            if (postContent.Text == "")
            {
                MessageBox.Show("Please enter a message!");
            }
            else
            {
                this.postPopup.IsOpen = false;

                //Used to check if a user is attempting to post in the refugee camp.
                //Which DOES work... so we'll just stop them.
                int forumId = _viewModel.Thread != null ? _viewModel.Thread.ThreadID : -1;

                //TODO: status
                MainPage.api.PostInThread(forumId, _threadId, postContent.Text.Replace("\r\n", "\r").Replace("\r", "\r\n"), msg =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            _viewModel.ChangePage(PageDirection.REFRESH);
                            //TODO: jump to bottom
                        });
                }, (err, ex) =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(err);
                        Logger.WriteLine(ex);
                    });
                });
            }
        }

        private void JumpToClick(object sender, RoutedEventArgs e)
        {
            this.jumpToPopup.IsOpen = false;
            SystemTray.BackgroundColor = _viewModel.BgColor;

            int page = -1;
            if (Int32.TryParse(pageNumberTb.Text, out page))
            {
                _viewModel.ChangePage(page);
            }
            pageNumberTb.Text = "";
        }

        private void WebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (sender == null)
                return;
            var browser = (WebBrowser)sender;

            int height = Convert.ToInt32(e.Value);
            double newHeight = height * 1.45;

            browser.Height = newHeight;
        }

        private void OnManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            this.Focus(); // set focus to current page
        }

        /*private void HTMLTextBox_ImageClick(object sender, MSPToolkit.Controls.ImageClickEventArgs e)
        {
            //TODO: something
            if (sender == null)
                return;

            Image img = (Image)sender;
            PopupImage.Source = img.Source;
            imagePopup.IsOpen = true;
        }*/

        /*private void HTMLTextBox_HyperlinkClick(object sender, MSPToolkit.Controls.HyperlinkClickEventArgs e)
        {
            if (sender == null)
                return;
            if (e.NavigationUri.OriginalString.Equals(""))
                return;

            var wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
            //Problem with bbcode. If a url is encased in quotes, it includes them. Hopefully this doesn't affect anything
            wbt.Uri = new Uri(e.NavigationUri.OriginalString.Replace("%22", ""));
            wbt.Show();
        }*/

        private void TextBlock_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        PostModel _selectedPost = null;

        private void quoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPost == null || this.postPopup.IsOpen || this.jumpToPopup.IsOpen || _viewModel.IsLoading)
                return;

            //To remove inner quotes
            /*BBCodeParser parser = new BBCodeParser(new [] {
                new BBTag("quote", "", "", false, true)
            });*/
            var parsed = _selectedPost.PageText; // parser.ToHtml(_selectedPost.PageText);
            if (parsed.Length > 1400)
                parsed = parsed.Substring(0, 1400); //Arbitrary length... just so we don't exceed 2048px limit...
            var quote = "[QUOTE=" + _selectedPost.Author.Name + ";" + _selectedPost.PostID + "]" + parsed + "[/QUOTE]\r\n";
            postContent.Text = quote;

            //messageScrollViewer.ScrollToVerticalOffset(messageScrollViewer.ActualHeight);

            this.postPopup.IsOpen = true;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ListBoxItem contextMenuListItem = PostsList.ItemContainerGenerator.ContainerFromItem((sender as ContextMenu).DataContext) as ListBoxItem;
            _selectedPost = contextMenuListItem.DataContext as PostModel;
        }
    }
}