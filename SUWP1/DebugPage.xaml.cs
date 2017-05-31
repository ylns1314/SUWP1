using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http.Filters;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SUWP1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugPage : Page
    {
        private List<Structures.Post> postList;
        private string console;
        private double wbPostMaxWidth = 100;
        WebClient.JsonHandler jh;
        public UI.ViewModelThread viewModel { get; set; }
        /// <summary>
        /// Constructor of the debug page
        /// </summary>
        public DebugPage()
        {
            this.InitializeComponent();
            this.viewModel = new UI.ViewModelThread();
            jh = new WebClient.JsonHandler();
            console = "click any buttons";
            var filter = new HttpBaseProtocolFilter();
            wbPostMaxWidth = lvPostList.ActualWidth - 200;
            this.Bindings.Update();
        }
        
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }

        private async void btnForumList_Click(object sender, RoutedEventArgs e)
        {
            //console = await jh.getForumList();
            this.Bindings.Update();
        }

        private async void btnAuth_Click(object sender, RoutedEventArgs e)
        {
            console = await jh.getAuth();
            this.Bindings.Update();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            console = await jh.login("ylns1314", "belldandy4869");
        }

        private async void btnThreadList_Click(object sender, RoutedEventArgs e)
        {
            var fid = Int32.Parse(tbFid.Text);
            //console = await jh.getThreadList(fid, 1);
            this.Bindings.Update();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            console = tbFid.Text;
            this.Bindings.Update();
        }

        private async void btnPostList_Click(object sender, RoutedEventArgs e)
        {
            var fid = Int32.Parse(tbFid.Text);
            var tid = Int32.Parse(tbTid.Text);
            //viewModel.addPosts(tid, 1);
            var indexLast = (postList.Count == 0) ? 0 : postList.Count - 1;
            Debug.WriteLine("numPosts = " + postList.Count);
            //wvPost.NavigateToString(postList[indexLast].htmlMsg);
            this.Bindings.Update();
        }

        private void wvPostMsg_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Helpers.WebViewAutoFit.ResizeToContent(sender);
        }
    }
}
