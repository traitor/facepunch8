using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8.API
{
    public class Forum
    {
        public Forum()
        {
            SubForums = new List<Forum>();
        }

        [DataMember]
        public int ForumID { get; set; }
        [DataMember]
        public int ParentID { get; set; }
        [DataMember]
        public int OrderPriority { get; set; }
        [DataMember]
        public Forum ParentForum { get; set; }
        [DataMember]
        public List<Forum> SubForums { get; private set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int PostCount { get; set; }
        [DataMember]
        public int ThreadCount { get; set; }
        [DataMember]
        public Thread LatestThread { get; set; }
    }
}
