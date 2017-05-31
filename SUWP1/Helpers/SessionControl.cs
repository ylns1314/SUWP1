using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using Windows.Web.Http.Filters;
using Windows.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace SUWP1.Helpers
{
    static class SessionControl
    {
        private static HttpClient hc;
        private static ResourceLoader loader;

        static SessionControl()
        {
            hc = new HttpClient();
            
            loader = ResourceLoader.GetForCurrentView("S1APIs");
        }

        public static async Task<string> Login(string username, string pwd)
        {
            var form = new HttpMultipartFormDataContent();
            form.Add(new HttpStringContent(username), "username");
            form.Add(new HttpStringContent(pwd), "password");
            var url_login_string = loader.GetString("URL_BASE_MOBILE_API") + loader.GetString("URL_LOGIN");
            var url_login = new Uri(url_login_string);
            var response = await hc.PostAsync(url_login,form);
            Debug.WriteLine(response.Content);
            Debug.WriteLine(response.Headers);
            var filter = new HttpBaseProtocolFilter();
            var cookieCollection = filter.CookieManager.GetCookies(response.RequestMessage.RequestUri);
            var result = "";
            result = cookieCollection.Count + " cookies found.\r\n";
            foreach (var cookie in cookieCollection)
            {
                result += "--------------------\r\n";
                result += "Name: " + cookie.Name + "\r\n";
                result += "Domain: " + cookie.Domain + "\r\n";
                result += "Path: " + cookie.Path + "\r\n";
                result += "Value: " + cookie.Value + "\r\n";
                result += "Expires: " + cookie.Expires + "\r\n";
                result += "Secure: " + cookie.Secure + "\r\n";
                result += "HttpOnly: " + cookie.HttpOnly + "\r\n";
            }

            return result;
            //return response.Content + "\n==========\n" + response.Headers.ToString();
        }

    }
}
