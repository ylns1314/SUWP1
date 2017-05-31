using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Data.Json;
using Windows.Data.Html;
using Windows.ApplicationModel.Resources;

namespace SUWP1.WebClient
{
    class JsonHandler
    {
        string arc = "";
        HttpClient hc;
        ResourceLoader loaderURL;
        string strUrlBaseApi = "";
        string strForumList = "";
        string strAuth = "";
        string strThreadList = "";
        string strPostList = "";
        string tpp = "30"; /* NUM_PER_PAGE is a STRING! */

        public JsonHandler()
        {
            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            hc = new HttpClient(filter);
            loaderURL = ResourceLoader.GetForCurrentView("S1APIs");
            strUrlBaseApi = loaderURL.GetString("URL_BASE_MOBILE_API");
            strForumList = loaderURL.GetString("FORUM_LIST");
            strThreadList = loaderURL.GetString("THREAD_LIST");
            strPostList = loaderURL.GetString("POST_LIST");
            strAuth = loaderURL.GetString("URL_AUTH");
        }

        public async Task<string> getAuth()
        {
            var result = "";
            var url_auth = strUrlBaseApi + strAuth;
            var strJsoning = await hc.GetStringAsync(new Uri(url_auth));
            Debug.WriteLine(strJsoning);
            result += strJsoning + "\n";
            return result;
        }

        public async Task<string> login(string username, string pwd)
        {
            var result = await Helpers.SessionControl.Login(username, pwd);
            return result;
        }

        public async Task<List<Structures.Forum>> getForumList()
        {
            var result = new List<Structures.Forum>();
            var strJsoning = await hc.GetStringAsync(new Uri(strUrlBaseApi + strForumList));
            Debug.WriteLine(strJsoning);
            var jo = JsonObject.Parse(strJsoning);
            var forumsArray = jo["Variables"].GetObject()["forumlist"].GetArray();
            var numForums = forumsArray.Count;

            foreach (JsonObject tmp in forumsArray)
            {
                var f = new Structures.Forum();
                f.fid = tmp["fid"].GetString();
                f.name = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tmp["name"].GetString());
                result.Add(f);
            }
            //result += "numForum = " + joW["Variables"].GetObject()["forumlist"].GetArray().Count + "\n";
            //for(int i = 0;i < joW["Variables"].GetObject()["forumlist"].GetArray().Count;i++)
            //{
            //    JsonObject tmp = joW["Variables"].GetObject()["forumlist"].GetArray()[i].GetObject();
            //    result += "fid: " + tmp["fid"].GetString() + "\tname: " + tmp["name"].GetString() + "\n";
            //}
            return result;
        }

        public async Task<List<Structures.Thread>> getThreadList(int fid, int numPage)
        {

            var strUrl = strUrlBaseApi + strThreadList + "&fid=" + fid + "&tpp=" + tpp + "&page=" + numPage;
            var strJson = await hc.GetStringAsync(new Uri(strUrl));
            var jo = JsonObject.Parse(strJson);
            var threadsArray = jo["Variables"].GetObject()["forum_threadlist"].GetArray();
            var numThreads = threadsArray.Count;
            var result = new List<Structures.Thread>();
            arc = strJson;

            // sort threads by lastpost time in descending order
            var threads = new Helpers.ComparableJsonContainer[numThreads];
            for(var i = 0;i < numThreads;i++)
            {
                var tmp = threadsArray[i].GetObject();
                var threadTuple = new Helpers.ComparableJsonContainer { obj = tmp, key = int.Parse(tmp["dblastpost"].GetString()) };
                threads[i] = threadTuple;
            }
            IEnumerable<Helpers.ComparableJsonContainer> sortedTTs = threads.OrderByDescending(tuple => tuple.key);
            foreach (var tuple in sortedTTs)
            {
                var t = new Structures.Thread();
                /// HtmlUtilities.ConvertToText toooooooooooooooooo slow!
                t.tid = tuple.obj["tid"].GetString();
                t.fid = fid.ToString();
                t.authorid = tuple.obj["authorid"].GetString();
                t.author = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["author"].GetString());
                t.subject = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["subject"].GetString());
                t.lastpost = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["lastpost"].GetString());
                t.lastposter = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["lastposter"].GetString());
                t.dateline = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["dateline"].GetString());
                t.replies = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["replies"].GetString());
                result.Add(t);
            }
            return result;
        }

        public async Task<List<Structures.Post>> getPostList(int tid, int numPage)
        {
            var strUrl = strUrlBaseApi + strPostList + "&tid=" + tid + "&ppp=" + tpp + "&page=" + numPage;
            var strJson = await hc.GetStringAsync(new Uri(strUrl));
            var jo = JsonObject.Parse(strJson);
            var postsArray = jo["Variables"].GetObject()["postlist"].GetArray();
            var numPosts = postsArray.Count;
            var result = new List<Structures.Post>();
            arc = strJson;
            
            // sort posts by lastpost time in ascending order (earlier in the front)
            var posts = new Helpers.ComparableJsonContainer[numPosts];
            for(var i = 0;i < numPosts;i++)
            {
                var tmp = postsArray[i].GetObject();
                var postTuple = new Helpers.ComparableJsonContainer { obj = tmp, key = int.Parse(tmp["dbdateline"].GetString()) };
                posts[i] = postTuple;
            }
            IEnumerable<Helpers.ComparableJsonContainer> sortedTTs = posts.OrderBy(tuple => tuple.key);
            foreach (var tuple in sortedTTs)
            {
                // replace all img attachments with real urls
                var resultTmp = Helpers.AttachSolver.solve(tuple.obj);
                // replace all html char entities
                resultTmp = Helpers.HtmlHelper.ReplaceHtmlCharEntities(resultTmp);
                // replace <br> with <br/> and <img ...> with <img .../>
                resultTmp = Helpers.HtmlHelper.CompleteTags(resultTmp);

                var p = new Structures.Post();
                p.htmlMsg = resultTmp;
                p.strUriAvatar = Helpers.AvatarFactory.getAvatar(tuple.obj["authorid"].GetString(), true);
                p.strUriAvatar_s = Helpers.AvatarFactory.getAvatar(tuple.obj["authorid"].GetString(), false);
                p.tid = tuple.obj["tid"].GetString();
                p.authorid = tuple.obj["authorid"].GetString();
                p.author = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["author"].GetString());
                p.dateline = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["dateline"].GetString());
                p.memberstatus = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["memberstatus"].GetString());
                p.status = Helpers.HtmlHelper.ReplaceHtmlCharEntities(tuple.obj["status"].GetString());
                result.Add(p);
            }
            return result;
        }
    }
}
