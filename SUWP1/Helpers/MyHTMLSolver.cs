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
    static class MyHTMLSolver
    {
        /// <summary>
        /// Remove HTML Chararcter entities in the given html text
        /// </summary>
        /// <param name="htmlText"></param>
        /// <returns>the clean version of htmlText</returns>
        public static string removeHTMLCharEntities(string htmlText)
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
            htmlText = htmlText.Replace("&quote;", "\"");
            htmlText = htmlText.Replace("&reg;", "®");
            return htmlText;
        }

        public static string completeTags(string htmlText)
        {
            // regex for all > without /
            Regex tagQuoteEnd = new Regex(@"(?!\/)>");
            htmlText = tagQuoteEnd.Replace(htmlText, "/>");
            return htmlText;
        }
    }
}
