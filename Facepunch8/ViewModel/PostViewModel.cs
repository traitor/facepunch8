using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Facepunch8.API;
using Facepunch8.Model;
using CodeKicker.BBCode;
using CaledosLab.Portable.Logging;

namespace Facepunch8.ViewModel
{
    [DataContract]
    public class PostViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private Thread _thread;
        private int _curPage;
        private object _collectionLock = new object();
        private BBCodeParser _parser;
        private bool _isLoading = false;

        [DataMember]
        public Thread Thread { set { _thread = value; } get { return _thread; } }

        [DataMember]
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
            }
        }

        public string Title { get { return _thread == null ? "" : _thread.Title; } }

        [DataMember]
        public int CurrentPage
        {
            get { return _curPage; }
            set
            {
                _curPage = value;
                NotifyPropertyChanged();
            }
        }

        public int PageCount { get { return _thread == null ? 0 : _thread.PageCount; } }

        [DataMember]
        public ObservableCollection<PostModel> PostsCollection
        {
            get;
            set;
        }

        static string GetUrlTagHrefAttributeValue(IAttributeRenderingContext attributeRenderingContext)
        {
            if (!string.IsNullOrWhiteSpace(attributeRenderingContext.AttributeValue))
                return attributeRenderingContext.AttributeValue; //explicit href attribute on url-Tag

            var tagContent = attributeRenderingContext.GetAttributeValueByID(BBTag.ContentPlaceholderName);
            return tagContent;
        }

        public PostViewModel()
        {
            PostsCollection = new ObservableCollection<PostModel>();

            //To remove the ;01238918 garbage
            Func<IAttributeRenderingContext, string> fixUpFunction = 
                (x) => {
                    return x.AttributeValue == null ? "" : x.AttributeValue.Split(';')[0];
                };
            Func<IAttributeRenderingContext, string> videoStuff =
                (x) =>
                {
                    return x.AttributeValue == null ? "" : x.AttributeValue.Split(';')[0];
                };

            //Image display settings.
            BBTag img, t;
            if (Settings.DisplayImages)
            {
                img = new BBTag("img", "<img src=\"${content}\" />", "", false, true);
                t = new BBTag("t", "<img src=\"${content}\" />", "", false, true);
            }
            else
            {
                img = new BBTag("img", "<a href=\"${content}\">${content}</a>", "", false, true);
                t = new BBTag("t", "", "<a href=\"${content}\">${content}</a>", false, true);
            }

            _parser = new BBCodeParser(new[]
            {
                new BBTag("b", "<b>", "</b>"), 
                new BBTag("h2", "<h2>", "</h2>"), 

                //Doesn't appear to work:
                new BBTag("del", "<span style='text-decoration:line-through'>", "</span>"), 

                new BBTag("i", "<span style=\"font-style:italic;\">", "</span>"), 
                new BBTag("u", "<span style=\"text-decoration:underline;\">", "</span>"), 
                new BBTag("editline", "<span style=\"text-decoration:underline;\">Edited on ", "</span>"), 
                new BBTag("highlight", "<span style=\"color: #417394;background: #FFEB90 none repeat-x;\">", "</span>"), 
                //new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"), 
                img,
                t,
                //Highlights that it's a quote with color...
                new BBTag("quote", "<p style=\"color: #417394;\"><b>${user}</b><br/>", "</p>", new BBAttribute("user", "", fixUpFunction, HtmlEncodingMode.UnsafeDontEncode), new BBAttribute("user", "user")),
                new BBTag("list", "<ul>", "</ul>"), 
                new BBTag("*", "<li>", "</li>", true, false), 

                //fix urls: http://bbcode.codeplex.com/workitem/9530
                new BBTag("url", "<a href=\"${href}\" rel=\"nofollow\">", "</a>",
                    new BBAttribute("href", "", GetUrlTagHrefAttributeValue),
                    new BBAttribute("href", "href", GetUrlTagHrefAttributeValue)),

                new BBTag("video", "<a href=\"${content}\">", "Youtube Video</a>", true, false, new BBAttribute("href", "", videoStuff), new BBAttribute("href", "href")), 
                new BBTag("media", "<a href=\"${content}\">${content}</a>", "", false, true), 
                new BBTag("hd", "<a href=\"${content}\">${content}</a>", "", false, true), 
                //what else...?
            });
        }

        public void Load(int threadId, int page)
        {
            var api = MainPage.api;
            CurrentPage = page;
            IsLoading = true;

            api.GetPosts(threadId, page, (posts, thread) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    lock (_collectionLock)
                    {
                        foreach (Post p in posts)
                        {
                            PostsCollection.Add(new PostModel(p));
                        }
                    }
                    _thread = thread;
                    //Update the UI
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("PageCount");
                    IsLoading = false;
                });
            }, (err, ex) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("Unable to load posts.");
                    Logger.WriteLine(ex);
                    IsLoading = false;
                });
            });
        }

        public void ChangePage(PageDirection dir)
        {
            if (dir == PageDirection.PREVIOUS)
            {
                if (CurrentPage > 1)
                    ChangePage(CurrentPage - 1);
            }
            else if (dir == PageDirection.NEXT)
            {
                if (CurrentPage < PageCount)
                    ChangePage(CurrentPage + 1);
            }
            else
                ChangePage(CurrentPage);
        }

        public void ChangePage(int page)
        {
            //Don't browse higher... TODO: maybe there's new pages?
            if (page > PageCount || page < 1)
                return;

            var api = MainPage.api;
            PostsCollection.Clear(); //TODO: temp until figure out way to auto scroll to top of list...
            IsLoading = true;

            api.GetPosts(_thread.ThreadID, page, (posts, thread) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    GC.Collect(5);

                    lock (_collectionLock)
                    {
                        CurrentPage = page;

                        PostsCollection.Clear();

                        foreach (Post p in posts)
                        {
                            PostsCollection.Add(new PostModel(p));
                        }
                    }

                    _thread = thread;

                    //Update the UI
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("PageCount");
                    IsLoading = false;
                });
            }, (err, ex) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("Unable to load posts.");
                    Logger.WriteLine(ex);
                    IsLoading = false;
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
