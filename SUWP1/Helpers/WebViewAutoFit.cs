using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SUWP1.Helpers
{
    /// <summary>
    /// Great thanks to http://jasonpoon.ca/2015/01/08/resizing-webview-to-its-content/
    /// </summary>
    static class WebViewAutoFit
    {
        /// <summary>
        /// The method runs 2 javascript commands to asyncly get the height and width for loading the web conetent without scrolling.
        /// </summary>
        /// <param name="webView"></param>
        public static async void ResizeToContent(this WebView webView)
        {
            var heightString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
            int height;
            if (int.TryParse(heightString, out height))
            {
                webView.Height = height;
            }

            var widthString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
            int width;
            if (int.TryParse(widthString, out width))
            {
                webView.Width = width;
            }
        }
    }
}
