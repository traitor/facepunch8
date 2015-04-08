using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8.API
{
    [DataContract]
    public class Thread
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public User Author { get; set; }
        [DataMember]
        public int ThreadID { get; set; }
        [DataMember]
        public int ForumID { get; set; }
        [DataMember]
        public bool IsOpen { get; set; }
        [DataMember]
        public bool IsStickied { get; set; }
        [DataMember]
        public bool Visible { get; set; }
        [DataMember]
        public int IconID { get; set; }
        [DataMember]
        public int NumberOfPosts { get; set; }
        [DataMember]
        public int NumberOfViews { get; set; }
        [DataMember]
        public int PageCount { get; set; }
        [DataMember]
        public User LatestPoster { get; set; }
        [DataMember]
        public int LastPostTimestamp { get; set; }
        [DataMember]
        public int ThreadCreationTimestamp { get; set; }
    }
}
