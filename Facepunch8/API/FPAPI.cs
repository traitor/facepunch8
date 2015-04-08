using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facepunch8.API
{
    [DataContract]
    public class FPAPI
    {
        public enum RequestType { GET, POST };
        public enum Status { OK, FAILURE };
        public const string BaseUrl = "http://lab.facepunch.com/api/";

        public delegate void SuccessfulCallback(string data);
        public delegate void LoginCallback(User user);
        public delegate void ThreadsCallback(List<Thread> threads);
        public delegate void PostsCallback(List<Post> threads, Thread thread);
        public delegate void ErrorCallback(string info, Exception ex);
        public delegate void WriteDataCallback(StreamWriter sw);

        [DataMember]
        public List<Forum> Forums { get; set; }

        [DataMember]
        public bool LoggedIn { get; set; }
        [DataMember]
        public User User { get; set; }

        [DataMember]
        public string SessionToken { set { _sessionToken = value; } get { return _sessionToken; } }

        [DataMember]
        public CookieContainer Cookies { get { return _cookies; } set { _cookies = value; } }

        private static readonly int[] forumOrder = new int[] { 
            3, 6, 60, 64, 403, 396, 51, 407, 46, 408, 409, 56, 398, 75, 316, 315, 389, 
            76, 11, 15, 198, 66, 65, 16, 197, 62, 36, 348, 110, 339, 277, 411, 412, 383, 189, /* HMM. */ 422, 415, 424, 418, 417, 419, 421, 425, 420, 
            /* DEVELOPERS */ 
            413, 240, 353, 38, 40, 
            /* HW & SW */
            228, 397, 374, 107, 243, 392, 262, 384, 385, 393, 401, 361, 394, 391, 388, 410, 390 };

        private string _sessionToken = "";
        private CookieContainer _cookies;

        public FPAPI()
        {
            _cookies = new CookieContainer();
            LoggedIn = false;
        }

        public FPAPI(User user, string sessionToken)
        {
            this.User = user;
            _sessionToken = sessionToken;
            LoggedIn = true;
        }

        public void GetForums(SuccessfulCallback scb, ErrorCallback ecb)
        {
            string url = "forum/list?";
            if (LoggedIn)
                url += "&_t=" + _sessionToken + "&_u=" + User.UserID;

            RequestData(url, RequestType.GET, wb => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string status = (string)root["status"]; //todo

                    Forums = new List<Forum>();

                    foreach (JObject obj in root["data"])
                    {
                        int forumId = (int)obj["forumid"];
                        int parentId = (int)obj["parentid"];
                        string title = (string)obj["title"];
                        string description = (string)obj["description"];
                        if (forumId == 51) //Remove html...
                            description = "Holy shit! It's the news!";
                        int postCount = (int)obj["postcount"];
                        int threadCount = (int)obj["threadcount"];
                        string threadTitle = (string)obj["thread_title"];
                        int threadId = (int)obj["thread_id"];
                        int threadDate = (int)obj["thread_date"];
                        int threadPostId = (int)obj["thread_postid"];
                        string author = (string)obj["thread_username"];
                        int userId = (int)obj["thread_userid"];

                        User user = new User() { 
                            Name = author, 
                            UserID = userId };

                        Thread thread = new Thread() { 
                            Author = user, 
                            LastPostTimestamp = threadDate, 
                            ThreadID = threadId, 
                            Title = threadTitle };

                        Forum forum = new Forum() { 
                            Description = description, 
                            LatestThread = thread, 
                            ParentID = parentId, 
                            ForumID = forumId,
                            PostCount = postCount, 
                            ThreadCount = threadCount, 
                            Title = title };

                        for (int i = 0; i < forumOrder.Length; i++)
                            if (forumOrder[i] == forum.ForumID)
                                forum.OrderPriority = i;

                        Forums.Add(forum);
                        
                    }

                    //Hacky way to make it ordered "right".
                    Forums = Forums.OrderBy(f => f.OrderPriority).ToList();//.ThenBy(f => f.ForumID).ToList();
                    //Forums.Sort((x, y) => { return x.ParentID.CompareTo(y.ParentID); });

                    scb("Success.");
                }, ecb);
        }

        public void GetThreads(int forumId, int page, ThreadsCallback scb, ErrorCallback ecb)
        {
            string url = "thread/list?forumid=" + forumId + (page > 1 ? "&page="+page : "");
            if (LoggedIn)
                url += "&_t=" + _sessionToken + "&_u=" + User.UserID;

            RequestData(url, RequestType.GET, w => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string statusText = (string)root["status"];
                    Status status = Status.FAILURE;
                    if (statusText.ToLower() == "ok")
                        status = Status.OK;

                    if (status == Status.OK)
                    {
                        List<Thread> threads = new List<Thread>();

                        foreach (JObject obj in root["data"]["threads"])
                        {
                            int threadId = (int)obj["threadid"];
                            //int forumId = (int)obj["forumid"];
                            string title = System.Net.HttpUtility.HtmlDecode((string)obj["title"]);
                            bool isOpen = ((int)obj["open"]) != 0;
                            bool isSticky = ((int)obj["sticky"]) == 1;
                            bool visible = ((int)obj["visible"]) == 1;
                            int iconId = (int)obj["iconid"];
                            int numPosts = (int)obj["numposts"];
                            int numViews = (int)obj["numviews"];
                            int pageCount = (int)obj["pagecount"];
                            if (pageCount == 0)
                                pageCount = 1; //Occurs only when there is only a single post
                            string authorName = (string)obj["author_name"];
                            int authorId = (int)obj["author_id"];
                            string latestPostAuthor = (string)obj["lastposter_name"];
                            int latestId = (int)obj["lastposter_id"];
                            int lastPost = (int)obj["lastpost"];
                            int dateline = (int)obj["dateline"];
                            User author = new User()
                                {
                                    Name = authorName,
                                    UserID = authorId
                                };
                            User latestPoster = new User()
                                {
                                    Name = latestPostAuthor,
                                    UserID = latestId
                                };
                            Thread thread = new Thread()
                                {
                                    ThreadID = threadId,
                                    ForumID = forumId,
                                    IsOpen = isOpen,
                                    IsStickied = isSticky,
                                    Visible = visible,
                                    IconID = iconId,
                                    NumberOfPosts = numPosts,
                                    NumberOfViews = numViews,
                                    PageCount = pageCount,
                                    Author = author,
                                    LatestPoster = latestPoster,
                                    Title = title,
                                    LastPostTimestamp = lastPost,
                                    ThreadCreationTimestamp = dateline
                                };

                            threads.Add(thread);
                        }

                        scb(threads);
                    }
                    else
                        ecb("Failure.", null);
                }, ecb);
        }

        public void Login(string username, string password, LoginCallback scb, ErrorCallback ecb)
        {
            RequestData("user/login/?password=" + password + "&username=" + username, RequestType.GET, w => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string statusText = (string)root["status"];
                    Status status = Status.FAILURE;
                    if (statusText.ToLower() == "ok")
                        status = Status.OK;

                    if (status == Status.OK)
                    {
                        //{"status":"ok","data":{"sessiontoken":"4aa6cbccca2ef5c7eac030af4ddb7bec","userid":111552,"username":"Hey0"}}
                        _sessionToken = (string)root["data"]["sessiontoken"];
                        int userId = (int)root["data"]["userid"];
                        string properUsername = (string)root["data"]["username"];

                        LoggedIn = true;
                        User = new User() { Name = properUsername, UserID = userId };

                        scb(User);
                    }
                    else
                    {
                        ecb((string)root["message"], null);
                    }
                }, ecb);
        }

        public void Logout()
        {
            _sessionToken = "";
            User = null;
            LoggedIn = false;
        }

        public Forum GetForum(int forumId)
        {
            return Forums.FirstOrDefault(f => f.ForumID == forumId);
        }

        public void GetPosts(int threadId, int page, PostsCallback scb, ErrorCallback ecb)
        {
            string url = "post/list?threadid=" + threadId + (page > 1 ? "&page=" + page : "");
            if (LoggedIn)
                url += "&_t=" + _sessionToken + "&_u=" + User.UserID;

            RequestData(url, RequestType.GET, w => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string status = (string)root["status"]; //todo

                    List<Post> posts = new List<Post>();

                    foreach (JObject obj in root["data"]["posts"])
                    {
                        int postId = (int)obj["postid"];
                        //threadid
                        string username = (string)obj["username"];
                        int userid = (int)obj["userid"];
                        int timestamp = (int)obj["dateline"];
                        string pageText = System.Net.HttpUtility.HtmlDecode((string)obj["pagetext"]);
                        bool visible = ((int)obj["visible"]) == 1;

                        User author = new User()
                            {
                                Name = username,
                                UserID = userid
                            };
                        Post post = new Post()
                            {
                                Author = author,
                                PageText = pageText,
                                Timestamp = timestamp,
                                Visible = visible,
                                PostID = postId
                            };
                        posts.Add(post);
                    }
                    //"thread":
                    //{"threadid":1363571,
                    //"title":"Valve: &quot;Bad communication is worse than none&quot;",
                    //"forumid":51,"open":1,
                    //"numposts":68,
                    //"lastpost_userid":245787,
                    //"lastpost_username":"LegndNikko",
                    //"lastpost_id":43903795,
                    //"thread_userid":546131,"thread_username":"PCGamesN","thread_postid":43903795
                    int numPostsPerPage = (int)root["data"]["forum"]["postsperpage"];
                    string threadTitle = (string)root["data"]["thread"]["title"];
                    int numPosts = (int)root["data"]["thread"]["numposts"];
                    int forumId = (int)root["data"]["thread"]["forumid"];
                    int authorId = (int)root["data"]["thread"]["thread_userid"];
                    string authorName = (string)root["data"]["thread"]["thread_username"];
                    int lastPosterId = (int)root["data"]["thread"]["lastpost_userid"];
                    string lastPosterName = (string)root["data"]["thread"]["lastpost_username"];
                    bool isOpen = (bool)root["data"]["thread"]["open"];
                    int pageCount = (int)Math.Ceiling((double)numPosts / numPostsPerPage);
                    User threadAuthor = new User() { Name = authorName, UserID = authorId };
                    User lastPoster = new User() { Name = lastPosterName, UserID = lastPosterId };

                    //ugly casting...
                    Thread thread = new Thread() { 
                        Title = HttpUtility.HtmlDecode(threadTitle), //fix this up
                        ThreadID = threadId, 
                        PageCount = pageCount,
                        Author = threadAuthor,
                        LatestPoster = lastPoster,
                        IsOpen = isOpen,
                        ForumID = forumId
                        };
                    //root["data"]["thread"]
                    //root["data"]["forum"]

                    scb(posts, thread);
                }, ecb);
        }

        public void PostInThread(int forumId, int threadId, string content, SuccessfulCallback scb, ErrorCallback ecb)
        {
            if (!LoggedIn)
            {
                ecb("Not logged in!", null);
                return;
            }
            if (forumId == 62) //Gold users can actually post in the camp - so yeah. no.
            {
                ecb("You cannot post in the Refugee Camp from this app.", null);
                return;
            }

            RequestData("author/post/?contents=" + content + "&threadid=" + threadId + "&_t=" + _sessionToken + "&_u=" + User.UserID, 
                RequestType.GET, w => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string statusText = (string)root["status"];
                    Status status = Status.FAILURE;
                    if (statusText.ToLower() == "ok")
                        status = Status.OK;

                    if (status == Status.OK)
                    {
                        scb("Successfully posted!");
                        //{"status":"ok","data":{"postid":43890609}}
                        //nothing useful.
                    }
                    else
                    {
                        ecb((string)root["message"], null);
                    }
                }, (err, ex) =>
                {
                    ecb("Failed to post", ex);
                });
        }

        public void PostThread(int forumId, string title, string content, SuccessfulCallback scb, ErrorCallback ecb)
        {
            if (!LoggedIn)
            {
                ecb("Not logged in!", null);
                return;
            }

            RequestData("author/thread/?contents=" + content + "&forumid=" + forumId + "&title=" + title + "&_t=" + _sessionToken + "&_u=" + User.UserID
                , RequestType.GET, w => { },
                data =>
                {
                    JObject root = JObject.Parse(data);
                    string statusText = (string)root["status"];
                    Status status = Status.FAILURE;
                    if (statusText.ToLower() == "ok")
                        status = Status.OK;

                    if (status == Status.OK)
                    {
                        scb("Successfully posted!");
                        //TODO: what's in this data
                    }
                    else
                    {
                        ecb((string)root["message"], null);
                    }
                }, (err, ex) =>
                {
                    ecb("Failed to post", ex);
                });
        }

        private void RequestData(string url, RequestType type, WriteDataCallback write, SuccessfulCallback success, ErrorCallback error)
        {
            try
            {
                //Prevent caching by adding the datetime. WP y u so dum...
                var request = WebRequest.CreateHttp(BaseUrl + url + "&nocache=" + DateTime.Now.Ticks.ToString());

                if (type == RequestType.POST)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                else
                    request.Method = "GET";
                request.CookieContainer = _cookies;
                request.UserAgent = request.UserAgent;

                if (type == RequestType.POST)
                {
                    request.BeginGetRequestStream(cb =>
                    {
                        using (var sr = new StreamWriter(request.EndGetRequestStream(cb)))
                        {
                            write(sr);
                        }

                        request.BeginGetResponse(
                               a =>
                               {
                                   try
                                   {
                                       var response = (HttpWebResponse)request.EndGetResponse(a);

                                       var responseStream = response.GetResponseStream();
                                       using (var sr = new StreamReader(responseStream))
                                       {
                                           success(sr.ReadToEnd());
                                       }
                                   }
                                   catch (WebException ex)
                                   {
                                       error("Web exception", ex);
                                   }
                               }, null);
                    }, null);
                }
                else
                {
                    request.BeginGetResponse(
                           a =>
                           {
                               try
                               {
                                   var response = (HttpWebResponse)request.EndGetResponse(a);

                                   var responseStream = response.GetResponseStream();
                                   using (var sr = new StreamReader(responseStream))
                                   {
                                       string result = sr.ReadToEnd();
                                       success(result);
                                   }
                               }
                               catch (WebException ex)
                               {
                                   error("GET exception", ex);
                               }
                           }, null);
                }
            }
            catch (WebException ex)
            {
                error("Web exception", ex);
            }
        }
    }
}
