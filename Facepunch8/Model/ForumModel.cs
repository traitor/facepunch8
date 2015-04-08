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
    public class ForumModel : INotifyPropertyChanged
    {
        private Forum _forum;
 
        public ForumModel(Forum forum)
        {
            _forum = forum;
        }

        [DataMember]
        public Forum Forum { get { return _forum; } set { _forum = value; } }

        public int ForumID { get { return _forum.ForumID; } }
        public int ParentID { get { return _forum.ParentID; }  }
        public Forum ParentForum { get { return _forum.ParentForum; }  }
        public List<Forum> SubForums { get { return _forum.SubForums; } }
        public string Title { get { return _forum.Title; }  }
        public string Description { get { return _forum.Description; }  }
        public int PostCount { get { return _forum.PostCount; }  }
        public int ThreadCount { get { return _forum.ThreadCount; }  }
        public Thread LatestThread { get { return _forum.LatestThread; }  }

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
