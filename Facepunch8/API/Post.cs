using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8.API
{
    [DataContract]
    public class Post
    {
        [DataMember]
        public int PostID { get; set; }
        [DataMember]
        public User Author { get; set; }
        [DataMember]
        public int Timestamp { get; set; }
        [DataMember]
        public string PageText { get; set; }
        [DataMember]
        public bool Visible { get; set; }
    }
}
