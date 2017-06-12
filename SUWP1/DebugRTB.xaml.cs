using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Storage;
using HtmlAgilityPack;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SUWP1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugRTB : Page
    {
        string console;
        public DebugRTB()
        {
            this.InitializeComponent();
            console = "no info\n";
        }

        private async Task<string> getJson()
        {
            WebClient.JsonHandler jh = new WebClient.JsonHandler();
            List<Structures.Post> posts = await jh.getPostList(1317876, 1);
            return posts[posts.Count - 1].htmlMsg;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }

        private async void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            console = await this.getJson();
            wbConsole.NavigateToString(console);
            RichTextBlock rtbConsole = new RichTextBlock();
            Paragraph p = new Paragraph();
            Span s = new Span();
            p.Inlines.Add(new Run() { Text = console });
            rtbConsole.Blocks.Add(p);
            svConsole.Content = rtbConsole;
            //hvConsole.Html = console;
            //hvConsole.Refresh();
            this.Bindings.Update();
        }
    }
}
