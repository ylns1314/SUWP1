using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Web.Http.Filters;
using SUWP1.Structures;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SUWP1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<Post> lstPosts;
        private List<Thread> lstThreads;
        WebClient.JsonHandler jh;
        public UI.ViewModelThread vmThread { get; set; }
        public UI.ViewModelSubForum vmSubForum { get; set; }
        public UI.ViewModelForum vmForum { get; set; }
        int postsCurrentPage, threadsCurrentPage, currentTid, currentFid;

        public MainPage()
        {
            this.InitializeComponent();
            postsCurrentPage = 1;
            threadsCurrentPage = 1;
            currentTid = 1330359;
            currentFid = 75;
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            this.vmThread = new UI.ViewModelThread();
            this.vmSubForum = new UI.ViewModelSubForum();
            this.vmForum = new UI.ViewModelForum();
            this.lstPosts = new List<Post>();
            this.lstThreads = new List<Thread>();
            jh = new WebClient.JsonHandler();
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();

            this.Bindings.Update();
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DebugPage));
        }

        private void btnDebugRTB_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DebugRTB));
        }

        private void btnWebView_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WebViewTest));
        }

        private void pgMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Bindings.Update();
        }

        private async void btnLoadThreads_Click(object sender, RoutedEventArgs e)
        {
            updateThreadList(true);
        }

        private async void btnLoadMoreThreads_Click(object sender, RoutedEventArgs e)
        {
            updateThreadList(false);
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var accountReader = ResourceLoader.GetForCurrentView("TestAccount");
            string cookiesReturn = await jh.login(accountReader.GetString("ID"), accountReader.GetString("PWD"));
            Debug.WriteLine(cookiesReturn);
        }

        private async void btnLoadPosts_Click(object sender, RoutedEventArgs e)
        {
            updatePostList(true);
        }

        private async void btnLoadMorePosts_Click(object sender, RoutedEventArgs e)
        {
            updatePostList(false);
        }

        private void lstRTBPostList_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            updatePostList(false);
        }

        private void lstThreadList_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            updateThreadList(false);
        }

        private void pullRefreshBoxPost_RefreshInvoked(DependencyObject sender, object args)
        {
            updatePostList(false);
        }

        private void pullRefreshBoxThread_RefreshInvoked(DependencyObject sender, object args)
        {
            updateThreadList(false);
        }

        private async void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            Thread t = sp.DataContext as Thread;
            currentTid = int.Parse(t.tid);
            updatePostList(true);
        }

        private async void updatePostList(bool isNew)
        {
            if(isNew)
            {
                postsCurrentPage = 1;
                lstPosts = await jh.getPostList(currentTid, postsCurrentPage++);
                vmThread.newPosts(lstPosts);
            }
            else
            {
                List<Post> tmp = await jh.getPostList(currentTid, postsCurrentPage++);
                lstPosts.Concat(tmp);
                vmThread.addPosts(tmp);
            }
        }

        private async void updateThreadList(bool isNew)
        {
            if(isNew)
            {
                threadsCurrentPage = 1;
                lstThreads = await jh.getThreadList(currentFid, threadsCurrentPage++);
                vmSubForum.newThreads(lstThreads);
            }
            else
            {
                List<Thread> tmp = await jh.getThreadList(currentFid, threadsCurrentPage++);
                lstThreads.Concat(tmp);
                vmSubForum.addThreads(tmp);
            }
        }
    }
}
