using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Facepunch8.API;

namespace Facepunch8.Model
{
    [DataContract]
    public class ThreadModel : INotifyPropertyChanged
    {
        private Thread _thread;

        public ThreadModel(Thread thread)
        {
            _thread = thread;
        }

        [DataMember]
        public Thread Thread { get { return _thread; } set { _thread = value; } }

        public string Title { get { return _thread.Title; }  }
        public User Author { get { return _thread.Author; }  }
        public int ThreadID { get { return _thread.ThreadID; }  }
        public int ForumID { get { return _thread.ForumID; }  }
        public bool IsOpen { get { return _thread.IsOpen; }  }
        public bool IsStickied { get { return _thread.IsStickied; }  }
        public bool Visible { get { return _thread.Visible; }  }
        public int IconID { get { return _thread.IconID; }  }
        public int NumberOfPosts { get { return _thread.NumberOfPosts; } }
        public int NumberOfViews { get { return _thread.NumberOfViews; } }
        public int PageCount { get { return _thread.PageCount; } }
        public User LatestPoster { get { return _thread.LatestPoster; } }
        public int LastPostTimestamp { get { return _thread.LastPostTimestamp; } }
        public int ThreadCreationTimestamp { get { return _thread.ThreadCreationTimestamp; } }

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
