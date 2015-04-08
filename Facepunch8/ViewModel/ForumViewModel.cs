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

namespace Facepunch8.ViewModel
{
    [DataContract]
    public class ForumViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private bool _isLoading = false;

        [DataMember]
        public ObservableCollection<ForumModel> ForumsCollection
        {
            get;
            set;
        }

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

        public ForumViewModel()
        {
            ForumsCollection = new ObservableCollection<ForumModel>();
        }

        public void Load()
        {
            var api = MainPage.api;
            IsLoading = true;

            api.GetForums(info =>
            {
                var forums = api.Forums;

                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var isGold = false;
                        foreach (Forum f in forums)
                            if (f.ForumID == 62)
                                isGold = true; //Onyl golds can see refugee camp

                        foreach (Forum f in forums)
                        {
                            if (f.ParentID == 407 || f.ForumID == 407)
                            {
                                if (isGold)
                                    ForumsCollection.Add(new ForumModel(f));
                            }
                            else
                                ForumsCollection.Add(new ForumModel(f));
                        }
                        IsLoading = false;
                    });
            }, (err, ex) =>
            {

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
