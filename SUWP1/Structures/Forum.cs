using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUWP1.Structures
{
    public class Forum
    {
        public string fid { get; set; }
        public string name { get; set; }
    }
    public class ForumGroup
    {
        public string groupid { get; set; }
        public string grouptitle { get; set; }
        public string allowvisit { get; set; }
        public string allowsendpm { get; set; }
        public string allowinvite { get; set; }
        public string allowmailinvite { get; set; }
        public string allowpost { get; set; }
        public string allowreply { get; set; }
        public string allowpostpoll { get; set; }
        public string allowpostreward { get; set; }
        public string allowposttrade { get; set; }
        public string allowpostactivity { get; set; }
        public string allowdirectpost { get; set; }
        public string allowgetattach { get; set; }
        public string allowgetimage { get; set; }
        public string allowpostattach { get; set; }
        public string allowpostimage { get; set; }
        public string allowvote { get; set; }
        public string allowsearch { get; set; }
        public string allowcstatus { get; set; }
        public string allowinvisible { get; set; }
        public string allowtransfer { get; set; }
        public string allowsetreadperm { get; set; }
        public string allowsetattachperm { get; set; }
        public string allowposttag { get; set; }
        public string allowhidecode { get; set; }
        public string allowhtml { get; set; }
        public string allowanonymous { get; set; }
        public string allowsigbbcode { get; set; }
        public string allowsigimgcode { get; set; }
        public string allowmagics { get; set; }
        public string allowpostdebate { get; set; }
        public string allowposturl { get; set; }
        public string allowrecommend { get; set; }
        public string allowpostrushreply { get; set; }
        public string allowcomment { get; set; }
        public string allowcommentarticle { get; set; }
        public string allowblog { get; set; }
        public string allowdoing { get; set; }
        public string allowupload { get; set; }
        public string allowshare { get; set; }
        public string allowblogmod { get; set; }
        public string allowdoingmod { get; set; }
        public string allowuploadmod { get; set; }
        public string allowsharemod { get; set; }
        public string allowcss { get; set; }
        public string allowpoke { get; set; }
        public string allowfriend { get; set; }
        public string allowclick { get; set; }
        public string allowmagic { get; set; }
        public string allowstat { get; set; }
        public string allowstatdata { get; set; }
        public string allowviewvideophoto { get; set; }
        public string allowmyop { get; set; }
        public string allowbuildgroup { get; set; }
        public string allowgroupdirectpost { get; set; }
        public string allowgroupposturl { get; set; }
        public string allowpostarticle { get; set; }
        public string allowdownlocalimg { get; set; }
        public string allowdownremoteimg { get; set; }
        public string allowpostarticlemod { get; set; }
        public string allowspacediyhtml { get; set; }
        public string allowspacediybbcode { get; set; }
        public string allowspacediyimgcode { get; set; }
        public string allowcommentpost { get; set; }
        public string allowcommentitem { get; set; }
        public string allowcommentreply { get; set; }
        public string allowreplycredit { get; set; }
        public string allowsendallpm { get; set; }
        public string allowsendpmmaxnum { get; set; }
        public string allowmediacode { get; set; }
        public string allowbegincode { get; set; }
        public string allowat { get; set; }
        public string allowsetpublishdate { get; set; }
        public string allowfollowcollection { get; set; }
        public string allowcommentcollection { get; set; }
        public string allowcreatecollection { get; set; }
        public string allowimgcontent { get; set; }
    }

    public class ForumCatlist
    {
        public string fid { get; set; }
        public string name { get; set; }
        public List<string> forums { get; set; }
    }

    public class Forumlist
    {
        public string fid { get; set; }
        public string name { get; set; }
        public string threads { get; set; }
        public string posts { get; set; }
        public string todayposts { get; set; }
        public string description { get; set; }
    }

    public class ForumVariables
    {
        public string cookiepre { get; set; }
        public string auth { get; set; }
        public string saltkey { get; set; }
        public string member_uid { get; set; }
        public string member_username { get; set; }
        public string groupid { get; set; }
        public string formhash { get; set; }
        public object ismoderator { get; set; }
        public string readaccess { get; set; }
        public string member_email { get; set; }
        public string member_credits { get; set; }
        public string setting_bbclosed { get; set; }
        public ForumGroup group { get; set; }
        public List<ForumCatlist> catlist { get; set; }
        public List<Forumlist> forumlist { get; set; }
    }

    public class ForumRootObject
    {
        public string Version { get; set; }
        public string Charset { get; set; }
        public ForumVariables Variables { get; set; }
    }
}
