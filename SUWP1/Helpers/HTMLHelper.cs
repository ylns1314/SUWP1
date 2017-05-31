using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace SUWP1.Helpers
{
    static class HtmlHelper
    {
        /// <summary>
        /// Replace HTML Chararcter entities in the given html text
        /// to corresponding symbolds.
        /// </summary>
        /// <param name="htmlText"></param>
        /// <returns>the clean version of htmlText</returns>
        public static string ReplaceHtmlCharEntities(string htmlText)
        {
            htmlText = htmlText.Replace("&nbsp;", " ");
            htmlText = htmlText.Replace("&lt;", "<");
            htmlText = htmlText.Replace("&gt;", ">");
            htmlText = htmlText.Replace("&amp;", "&");
            htmlText = htmlText.Replace("&cent;", "¢");
            htmlText = htmlText.Replace("&pound;", "£");
            htmlText = htmlText.Replace("&yen;", "¥");
            htmlText = htmlText.Replace("&euro;", "€");
            htmlText = htmlText.Replace("&copy;", "©");
            htmlText = htmlText.Replace("&quot;", "\"");
            htmlText = htmlText.Replace("&reg;", "®");
            return htmlText;
        }

        /// <summary>
        /// Add "&lt;" to all unclosed "&gt;".
        /// </summary>
        /// <param name="htmlText"></param>
        /// <returns></returns>
        public static string CompleteTags(string htmlText)
        {
            // regex for all > without /
            var tagQuoteEnd = new Regex(@"(?!\/)>");
            htmlText = tagQuoteEnd.Replace(htmlText, "/>");
            return htmlText;
        }
    }
}
