using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using HtmlAgilityPack;

namespace SUWP1.UI
{

    class HtmlToXamlProperties : DependencyObject
    {
        private static string url_base;

        static HtmlToXamlProperties()
        {
            var loaderApi = ResourceLoader.GetForCurrentView("S1APIs");
            url_base = loaderApi.GetString("URL_BASE");
        }

        /// <summary>
        /// pure Html Text without any modification, used for bindings on WebView
        /// </summary>
        public static readonly DependencyProperty HtmlMsgProperty =
            DependencyProperty.RegisterAttached("HtmlMsg", typeof(string), typeof(HtmlToXamlProperties), new PropertyMetadata(null, HtmlMsgChanged));

        public static void SetHtmlMsg(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlMsgProperty, value);
        }

        public static string GetHtmlMsg(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlMsgProperty);
        }

        public static void HtmlMsgChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wb = d as WebView;
            wb?.NavigateToString((string)e.NewValue);
        }


        /// <summary>
        /// Trigger for converting Html Text to Xaml for RichTextBlocks
        /// </summary>
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(HtmlToXamlProperties), new PropertyMetadata(null, HtmlChanged));

        /// <summary>
        /// sets the HTML property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        /// <summary>
        /// Gets the HTML property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        /// <summary>
        /// This is called when the HTML has changed so that we can generate RT content
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richText = d as RichTextBlock;
            if (richText == null) return;

            //Generate blocks
            var xhtml = e.NewValue as string;

            //string baselink = "";
            //if (richText.DataContext is BlogPostDataItem)
            //{
            //    BlogPostDataItem bp = richText.DataContext as BlogPostDataItem;
            //    baselink = "http://" + bp.Link.Host;
            //}

            var blocks = GenerateBlocksForHtml(xhtml);

            //Add the blocks to the RichTextBlock
            richText.Blocks.Clear();
            foreach (var b in blocks)
            {
                richText.Blocks.Add(b);
            }
        }


        private static List<Block> GenerateBlocksForHtml(string xhtml)
        {
            var bc = new List<Block>();
            var baselink = url_base;
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(xhtml);

                foreach (var img in doc.DocumentNode.Descendants("img"))
                {
                    if (!img.Attributes["src"].Value.StartsWith("http"))
                    {
                        img.Attributes["src"].Value = baselink + img.Attributes["src"].Value;
                    }
                }

                var b = GenerateParagraph(doc.DocumentNode);
                bc.Add(b);
            }
            catch (Exception ex)
            {
            }

            return bc;
        }

        /// <summary>
        /// Cleans HTML text for display in paragraphs
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// 
        private static string CleanText(string input)
        {
            //string clean = Windows.Data.Html.HtmlUtilities.ConvertToText(input);
            //clean = System.Net.WebUtility.HtmlEncode(clean);
            var clean = Helpers.HtmlHelper.ReplaceHtmlCharEntities(input);
            if (clean == "\0")
                clean = "\n";
            return clean;
        }

        private static Block GenerateBlockForTopNode(HtmlNode node)
        {
            return GenerateParagraph(node);
        }


        private static void AddChildren(Paragraph p, HtmlNode node)
        {
            var added = false;
            foreach (var child in node.ChildNodes)
            {
                var i = GenerateBlockForNode(child);
                if (i == null) continue;
                p.Inlines.Add(i);
                added = true;
            }
            if (!added)
            {
                p.Inlines.Add(new Run() { Text = CleanText(node.InnerText) });
            }
        }

        private static void AddChildren(Span s, HtmlNode node)
        {
            var added = false;
            
            foreach (var child in node.ChildNodes)
            {
                var i = GenerateBlockForNode(child);
                if (i != null)
                {
                    s.Inlines.Add(i);
                    added = true;
                }
            }
            if (!added)
            {
                s.Inlines.Add(new Run() { Text = CleanText(node.InnerText) });
            }
        }

        private static Inline GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name)
            {
                case "div":
                    return GenerateSpan(node);
                case "p":
                case "P":
                    return GenerateInnerParagraph(node);
                case "img":
                case "IMG":
                    return GenerateImage(node);
                case "a":
                case "A":
                    if (node.ChildNodes.Count >= 1 && (node.FirstChild.Name == "img" || node.FirstChild.Name == "IMG"))
                        return GenerateImage(node.FirstChild);
                    else
                        return GenerateHyperLink(node);
                case "li":
                case "LI":
                    return GenerateLI(node);
                case "b":
                case "B":
                case "strong":
                case "STRONG":
                    return GenerateBold(node);
                case "i":
                case "I":
                    return GenerateItalic(node);
                case "u":
                case "U":
                    return GenerateUnderline(node);
                case "br":
                case "BR":
                    return new LineBreak();
                case "span":
                case "Span":
                    return GenerateSpan(node);
                case "iframe":
                case "Iframe":
                    return GenerateIFrame(node);
                case "#text":
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        return new Run() { Text = CleanText(node.InnerText) };
                    break;
                case "h1":
                case "H1":
                    return GenerateH1(node);
                case "h2":
                case "H2":
                    return GenerateH2(node);
                case "h3":
                case "H3":
                    return GenerateH3(node);
                case "ul":
                case "UL":
                    return GenerateLI(node);
                case "blockquote":
                case "BLOCKQUOTE":
                    return GenerateBQ(node);
                case "font":
                case "FONT":
                    return GenerateFont(node);
                default:
                    return GenerateSpanWNewLine(node);
                    //if (!string.IsNullOrWhiteSpace(node.InnerText))
                    //    return new Run() { Text = CleanText(node.InnerText) };
                    //break;
            }
            return null;
        }

        private static Inline GenerateFont(HtmlNode node)
        {
            var s = new Span();
            foreach(var attr in node.Attributes)
            {
                switch (attr.Name) {
                    case "size":
                        var size = double.Parse(node.GetAttributeValue("size", s.FontSize.ToString()));
                        if(size != s.FontSize)
                            s.FontSize = size * 5.0;
                        break;
                    case "color":
                        var color = node.GetAttributeValue("color", "#233333");
                        if (color[0].Equals('#'))
                            s.Foreground = new SolidColorBrush(Helpers.HexColorSolver.GetColorFromHex(color));
                        else
                            s.Foreground = new SolidColorBrush((Color) XamlBindingHelper.ConvertValue(typeof(Color), color));
                        break;
                    case "face":
                    default:
                        break;
                }
            }
            AddChildren(s, node);
            return s;
        }
       
        /// <summary>
        /// Generate Quote Block
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Inline GenerateBQ(HtmlNode node)
        {
            var s = new Italic();
            s.Foreground = new SolidColorBrush(Colors.Gray);
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;           
        }
        
        /// <summary>
        /// Generate Indexed List
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Inline GenerateLI(HtmlNode node)
        {
            var s = new Span();
            var iui = new InlineUIContainer();
            //var ellipse = new Ellipse
            //{
            //    Fill = new SolidColorBrush(Colors.Black),
            //    Width = 6,
            //    Height = 6,
            //    Margin = new Thickness(-30, 0, 0, 1)
            //};
            //iui.Child = ellipse;
            s.Inlines.Add(iui);
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateImage(HtmlNode node)
        {
            var s = new Span();
            try
            {
                var iui = new InlineUIContainer();
                var sourceUri = System.Net.WebUtility.HtmlDecode(node.Attributes["src"].Value) ?? System.Net.WebUtility.HtmlDecode(node.Attributes["alt"].Value);
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(sourceUri, UriKind.Absolute)),
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                img.ImageOpened += img_ImageOpened;
                img.ImageFailed += img_ImageFailed;
                //img.Tapped += ScrollingBlogPostDetailPage.img_Tapped;
                iui.Child = img;
                s.Inlines.Add(iui);
                s.Inlines.Add(new LineBreak());
            }
            catch (Exception)
            {
                // ignored
            }
            return s;
        }

        static void img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var img = sender as Image;
        }

        static void img_ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            var bimg = img.Source as BitmapImage;
            Debug.WriteLine("height %d width %d", bimg.PixelHeight, bimg.PixelWidth);
            if (bimg.PixelWidth > 800 || bimg.PixelHeight > 600)
            {
                if (bimg.PixelWidth > 800)
                {
                    img.Width = 800;
                    img.Height = (800.0 / (double)bimg.PixelWidth) * bimg.PixelHeight;
                }
                // just don't rescale long images
                //if (img.Height > 600)
                //{
                //    img.Height = 600;
                //    img.Width = (600.0 / (double)img.Height) * img.Width;
                //}
            }
            else
            {
                img.Height = bimg.PixelHeight;
                img.Width = bimg.PixelWidth;
            }
            //img.Tapped += img_tapped;
        }

        static void img_tapped(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            img.RenderTransform = new ScaleTransform() { ScaleX = 2, ScaleY = 2 };
        }

        private static Inline GenerateHyperLink(HtmlNode node)
        {
            var s = new Span();
            var iui = new InlineUIContainer();
            var hb = new HyperlinkButton() { NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute), Content = CleanText(node.InnerText) };

            //if (node.ParentNode != null && (node.ParentNode.Name == "li" || node.ParentNode.Name == "LI"))
            //    hb.Style = (Style)Application.Current.Resources["RTLinkLI"];
            //else if ((node.NextSibling == null || string.IsNullOrWhiteSpace(node.NextSibling.InnerText)) && (node.PreviousSibling == null || string.IsNullOrWhiteSpace(node.PreviousSibling.InnerText)))
            //    hb.Style = (Style)Application.Current.Resources["RTLinkOnly"];
            //else
            //    hb.Style = (Style)Application.Current.Resources["RTLink"];

            iui.Child = hb;
            s.Inlines.Add(iui);
            return s;
        }

        private static Inline GenerateIFrame(HtmlNode node)
        {
            try
            {
                var s = new Span();
                s.Inlines.Add(new LineBreak());
                var iui = new InlineUIContainer();
                var ww = new WebView() { Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute), Width = Int32.Parse(node.Attributes["width"].Value), Height = Int32.Parse(node.Attributes["height"].Value) };
                iui.Child = ww;
                s.Inlines.Add(iui);
                s.Inlines.Add(new LineBreak());
                return s;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static Block GenerateTopIFrame(HtmlNode node)
        {
            try
            {
                var p = new Paragraph();
                var iui = new InlineUIContainer();
                var ww = new WebView() { Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute), Width = Int32.Parse(node.Attributes["width"].Value), Height = Int32.Parse(node.Attributes["height"].Value) };
                iui.Child = ww;
                p.Inlines.Add(iui);
                return p;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static Inline GenerateBold(HtmlNode node)
        {
            var b = new Bold();
            AddChildren(b, node);
            return b;
        }

        private static Inline GenerateUnderline(HtmlNode node)
        {
            var u = new Underline();
            AddChildren(u, node);
            return u;
        }

        private static Inline GenerateItalic(HtmlNode node)
        {
            var i = new Italic();
            AddChildren(i, node);
            return i;
        }

        private static Block GenerateParagraph(HtmlNode node)
        {
            var p = new Paragraph();
            AddChildren(p, node);
            return p;
        }

        private static Inline GenerateInnerParagraph(HtmlNode node)
        {
            var s = new Span();
            s.Inlines.Add(new LineBreak());
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateSpan(HtmlNode node)
        {
            var s = new Span();
            AddChildren(s, node);
            return s;
        }

        private static Inline GenerateSpanWNewLine(HtmlNode node)
        {
            var s = new Span();
            AddChildren(s, node);
            if (s.Inlines.Count > 0)
                s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Span GenerateH3(HtmlNode node)
        {
            var s = new Span();
            s.Inlines.Add(new LineBreak());
            var bold = new Bold();
            var r = new Run() { Text = CleanText(node.InnerText) };
            bold.Inlines.Add(r);
            s.Inlines.Add(bold);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateH2(HtmlNode node)
        {
            var s = new Span() { FontSize = 24 };
            s.Inlines.Add(new LineBreak());
            var r = new Run() { Text = CleanText(node.InnerText) };
            s.Inlines.Add(r);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateH1(HtmlNode node)
        {
            var s = new Span() { FontSize = 30 };
            s.Inlines.Add(new LineBreak());
            var r = new Run() { Text = CleanText(node.InnerText) };
            s.Inlines.Add(r);
            s.Inlines.Add(new LineBreak());
            return s;
        }
    }
}
