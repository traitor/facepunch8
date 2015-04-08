using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Facepunch8.API;

namespace Facepunch8.Model
{
    [DataContract]
    public class PostModel : INotifyPropertyChanged
    {
        private Post _post;
        private BitmapImage _avatar;

        public PostModel(Post post)
        {
            Post = post;
        }

        [DataMember]
        public Post Post { get { return _post; } 
            set 
            { 
                _post = value;

                //Helps for tombstoning to reload avatars.
                AvatarCache.Instance.RetrieveAvatar(Post.Author.UserID, img =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Avatar = img;
                    });
                });
            } 
        }

        public int PostID { get { return _post.PostID; } }
        public User Author { get { return _post.Author; } }
        public int Timestamp { get { return _post.Timestamp; } }
        public string PageText { get { return _post.PageText; } }
        public bool Visible { get { return _post.Visible; } }

        public BitmapImage Avatar
        {
            get
            {
                return _avatar;
            }
            set
            {
                _avatar = value;
                NotifyPropertyChanged();
            }
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
