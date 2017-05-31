using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.UI.Xaml.Documents;

namespace SUWP1.Structures
{
    public class Post 
    {
        public HtmlDocument htmlD { get; set; }
        public string htmlMsg { get; set; }
        public List<Block> RTBMsg { get; set; }
        public string strUriAvatar { get; set; }
        public string strUriAvatar_s { get; set; }
        public string pid { get; set; }
        public string tid { get; set; }
        public string first { get; set; }
        public string author { get; set; }
        public string authorid { get; set; }
        public string dateline { get; set; }
        public string message { get; set; }
        public string anonymous { get; set; }
        public string attachment { get; set; }
        public string status { get; set; }
        public string username { get; set; }
        public string adminid { get; set; }
        public string groupid { get; set; }
        public string memberstatus { get; set; }
        public string number { get; set; }
        public string dbdateline { get; set; }
        public List<string> attachlist { get; set; }
        public List<string> imagelist { get; set; }
    }
}
