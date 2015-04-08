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
using Microsoft.Phone.Shell;
using CaledosLab.Portable.Logging;

namespace Facepunch8.ViewModel
{
    public enum PageDirection { PREVIOUS, NEXT, REFRESH };

    [DataContract]
    public class ThreadViewModel : BaseViewModel, INotifyPropertyChanged
    {
        [DataMember]
        public ObservableCollection<ThreadModel> ThreadsCollection
        {
            get;
            set;
        }

        private Forum _forum;
        private int _curPage;
        private bool _isLoading = false;

        [DataMember]
        public int CurrentPage { get { return _curPage; } set { _curPage = value; } }

        [DataMember]
        public Forum Forum { get { return _forum; } set { _forum = value; } }

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

        /// <summary>
        /// Just for the .xaml binding
        /// </summary>
        public string Title
        {
            get { return (_forum == null ? "" : _forum.Title); }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public ThreadViewModel()
        {
            ThreadsCollection = new ObservableCollection<ThreadModel>();
        }

        public void Load(Forum forum, int page)
        {
            _forum = forum;
            _curPage = page;
            var api = MainPage.api;
            Title = _forum.Title; //doesn't do anything other than notify .xaml
            IsLoading = true;

            api.GetThreads(forum.ForumID, page, result =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (Thread t in result)
                            ThreadsCollection.Add(new ThreadModel(t));
                        IsLoading = false;
                    });
            }, (err, ex) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("Unable to load threads.");
                    Logger.WriteLine(ex);
                    IsLoading = false;
                });
            });
        }

        public void ChangePage(PageDirection dir)
        {
            var api = MainPage.api;

            if (dir == PageDirection.PREVIOUS)
            {
                if (_curPage == 1)
                    return;
                _curPage--;
            }
            else if (dir == PageDirection.NEXT)
            {
                _curPage++;
            }

            //TODO page count
            ThreadsCollection.Clear();
            IsLoading = true;

            api.GetThreads(_forum.ForumID, _curPage, result =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    foreach (Thread t in result)
                        ThreadsCollection.Add(new ThreadModel(t));
                    IsLoading = false;
                });
            }, (err, ex) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Windows.MessageBox.Show("Unable to load threads.");
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
