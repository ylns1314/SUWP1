using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using System.IO;
using System.Reflection;
using HtmlAgilityPack;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Globalization;

namespace SUWP1.UI
{

    public class CustomProperties : DependencyObject
    {

        /// <summary>
        /// pure Html Text without any modification, used for bindings on WebView
        /// </summary>
        public static readonly DependencyProperty HtmlMsgProperty =
            DependencyProperty.RegisterAttached("HtmlMsg", typeof(string), typeof(CustomProperties), new PropertyMetadata(null, HtmlMsgChanged));

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
            WebView wb = d as WebView;
            if (wb != null)
            {
                wb.NavigateToString((string)e.NewValue);
            }
        }


        /// <summary>
        /// Trigger for converting Html Text to Xaml for RichTextBlocks
        /// </summary>
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(CustomProperties), new PropertyMetadata(null, HtmlChanged));

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
            RichTextBlock richText = d as RichTextBlock;
            if (richText == null) return;

            //Generate blocks
            string xhtml = e.NewValue as string;

            //string baselink = "";
            //if (richText.DataContext is BlogPostDataItem)
            //{
            //    BlogPostDataItem bp = richText.DataContext as BlogPostDataItem;
            //    baselink = "http://" + bp.Link.Host;
            //}

            List<Block> blocks = GenerateBlocksForHtml(xhtml);

            //Add the blocks to the RichTextBlock
            richText.Blocks.Clear();
            foreach (Block b in blocks)
            {
                richText.Blocks.Add(b);
            }
        }


        private static List<Block> GenerateBlocksForHtml(string xhtml)
        {
            List<Block> bc = new List<Block>();
            string baselink = "http://www.saraba1st.com/2b/";
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(xhtml);

                foreach (HtmlNode img in doc.DocumentNode.Descendants("img"))
                {
                    if (!img.Attributes["src"].Value.StartsWith("http"))
                    {
                        img.Attributes["src"].Value = baselink + img.Attributes["src"].Value;
                    }
                }

                Block b = GenerateParagraph(doc.DocumentNode);
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
            string clean = Helpers.HtmlHelper.ReplaceHtmlCharEntities(input);
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
            bool added = false;
            foreach (HtmlNode child in node.ChildNodes)
            {
                Inline i = GenerateBlockForNode(child);
                if (i != null)
                {
                    p.Inlines.Add(i);
                    added = true;
                }
            }
            if (!added)
            {
                p.Inlines.Add(new Run() { Text = CleanText(node.InnerText) });
            }
        }

        private static void AddChildren(Span s, HtmlNode node)
        {
            bool added = false;
            
            foreach (HtmlNode child in node.ChildNodes)
            {
                Inline i = GenerateBlockForNode(child);
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
                    Debug.WriteLine("this is a pic");
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
                    return GenerateUL(node);
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
            Span s = new Span();
            foreach(HtmlAttribute attr in node.Attributes)
            {
                switch (attr.Name) {
                    case "size":
                        double size = double.Parse(node.GetAttributeValue("size", s.FontSize.ToString()));
                        if(size != s.FontSize)
                            s.FontSize = size * 5.0;
                        break;
                    case "color":
                        string color = node.GetAttributeValue("color", "#233333");
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
        
        private static Inline GenerateBQ(HtmlNode node)
        {
            Italic s = new Italic();
            s.Foreground = new SolidColorBrush(Colors.Gray);
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;           
        }
        private static Inline GenerateLI(HtmlNode node)
        {
            Span s = new Span();
            InlineUIContainer iui = new InlineUIContainer();
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = new SolidColorBrush(Colors.Black);
            ellipse.Width = 6;
            ellipse.Height = 6;
            ellipse.Margin = new Thickness(-30, 0, 0, 1);
            iui.Child = ellipse;
            s.Inlines.Add(iui);
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateImage(HtmlNode node)
        {
            Span s = new Span();
            try
            {
                InlineUIContainer iui = new InlineUIContainer();
                var sourceUri = System.Net.WebUtility.HtmlDecode(node.Attributes["src"].Value);
                if (sourceUri == null) sourceUri = System.Net.WebUtility.HtmlDecode(node.Attributes["alt"].Value);
                Debug.WriteLine(sourceUri);
                Image img = new Image() { Source = new BitmapImage(new Uri(sourceUri, UriKind.Absolute)) };
                img.Stretch = Stretch.Uniform;
                img.VerticalAlignment = VerticalAlignment.Top;
                img.HorizontalAlignment = HorizontalAlignment.Left;
                img.ImageOpened += img_ImageOpened;
                img.ImageFailed += img_ImageFailed;
                //img.Tapped += ScrollingBlogPostDetailPage.img_Tapped;
                iui.Child = img;
                s.Inlines.Add(iui);
                s.Inlines.Add(new LineBreak());
            }
            catch (Exception ex)
            {
            }
            return s;
        }

        static void img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var i = 5;
        }

        static void img_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            BitmapImage bimg = img.Source as BitmapImage;
            if (bimg.PixelWidth > 800 || bimg.PixelHeight > 600)
            {
                img.Width = 800; img.Height = 600;
                if (bimg.PixelWidth > 800)
                {
                    img.Width = 800;
                    img.Height = (800.0 / (double)bimg.PixelWidth) * bimg.PixelHeight;
                }
                if (img.Height > 600)
                {
                    img.Height = 600;
                    img.Width = (600.0 / (double)img.Height) * img.Width;
                }
            }
            else
            {
                img.Height = bimg.PixelHeight;
                img.Width = bimg.PixelWidth;
            }
        }

        private static Inline GenerateHyperLink(HtmlNode node)
        {
            Span s = new Span();
            InlineUIContainer iui = new InlineUIContainer();
            HyperlinkButton hb = new HyperlinkButton() { NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute), Content = CleanText(node.InnerText) };

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
                Span s = new Span();
                s.Inlines.Add(new LineBreak());
                InlineUIContainer iui = new InlineUIContainer();
                WebView ww = new WebView() { Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute), Width = Int32.Parse(node.Attributes["width"].Value), Height = Int32.Parse(node.Attributes["height"].Value) };
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
                Paragraph p = new Paragraph();
                InlineUIContainer iui = new InlineUIContainer();
                WebView ww = new WebView() { Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute), Width = Int32.Parse(node.Attributes["width"].Value), Height = Int32.Parse(node.Attributes["height"].Value) };
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
            Bold b = new Bold();
            AddChildren(b, node);
            return b;
        }

        private static Inline GenerateUnderline(HtmlNode node)
        {
            Underline u = new Underline();
            AddChildren(u, node);
            return u;
        }

        private static Inline GenerateItalic(HtmlNode node)
        {
            Italic i = new Italic();
            AddChildren(i, node);
            return i;
        }

        private static Block GenerateParagraph(HtmlNode node)
        {
            Paragraph p = new Paragraph();
            AddChildren(p, node);
            return p;
        }

        private static Inline GenerateUL(HtmlNode node)
        {
            Span s = new Span();
            s.Inlines.Add(new LineBreak());
            AddChildren(s, node);
            return s;
        }

        private static Inline GenerateInnerParagraph(HtmlNode node)
        {
            Span s = new Span();
            s.Inlines.Add(new LineBreak());
            AddChildren(s, node);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateSpan(HtmlNode node)
        {
            Span s = new Span();
            AddChildren(s, node);
            return s;
        }

        private static Inline GenerateSpanWNewLine(HtmlNode node)
        {
            Span s = new Span();
            AddChildren(s, node);
            if (s.Inlines.Count > 0)
                s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Span GenerateH3(HtmlNode node)
        {
            Span s = new Span();
            s.Inlines.Add(new LineBreak());
            Bold bold = new Bold();
            Run r = new Run() { Text = CleanText(node.InnerText) };
            bold.Inlines.Add(r);
            s.Inlines.Add(bold);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateH2(HtmlNode node)
        {
            Span s = new Span() { FontSize = 24 };
            s.Inlines.Add(new LineBreak());
            Run r = new Run() { Text = CleanText(node.InnerText) };
            s.Inlines.Add(r);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        private static Inline GenerateH1(HtmlNode node)
        {
            Span s = new Span() { FontSize = 30 };
            s.Inlines.Add(new LineBreak());
            Run r = new Run() { Text = CleanText(node.InnerText) };
            s.Inlines.Add(r);
            s.Inlines.Add(new LineBreak());
            return s;
        }

        #region old stuff

        private static XsltProcessor Html2XamlProcessor;

        private static async Task<string> ConvertHtmlToXamlRichTextBlock(string xhtml)
        {
            // Load XHTML fragment as XML document
            XmlDocument xhtmlDoc = new XmlDocument();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // In design mode we swallow all exceptions to make editing more friendly
                try { xhtmlDoc.LoadXml(xhtml); }
                catch { } // For some reason code in catch is not executed when an exception occurs in design mode, so we can't display a friendly error here.
            }
            else
            {
                // When not in design mode, we let the application handle any exceptions
                xhtmlDoc.LoadXml(xhtml);
            }

            if (Html2XamlProcessor == null)
            {
                // Read XSLT. In design mode we cannot access the xslt from the file system (with Build Action = Content), 
                // so we use it as an embedded resource instead:
                Assembly assembly = typeof(CustomProperties).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("SocialMediaDashboard.W8.Common.RichTextBlockHtml2Xaml.xslt"))
                {
                    StreamReader reader = new StreamReader(stream);
                    string content = await reader.ReadToEndAsync();
                    XmlDocument html2XamlXslDoc = new XmlDocument();
                    html2XamlXslDoc.LoadXml(content);
                    Html2XamlProcessor = new XsltProcessor(html2XamlXslDoc);
                }
            }

            // Apply XSLT to XML
            string xaml = Html2XamlProcessor.TransformToString(xhtmlDoc.FirstChild);
            return xaml;
        }

        /// <summary>
        /// Convert HTML to Rich textblock controls
        /// </summary>
        /// <param name="xhtml"></param>
        /// <returns></returns>
        private static async Task<string> ConvertHtmlToXamlRichTextBlock2(string xhtml)
        {
            string xaml = "<?xml version=\"1.0\"?><RichTextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">";

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(xhtml);

                foreach (HtmlNode node in doc.DocumentNode.ChildNodes)
                {
                    xaml += GenerateBlockForTopNode(node);
                }
            }
            catch (Exception ex)
            {
            }

            xaml += "</RichTextBlock>";

            return xaml;
        }

        private static string GenerateBlockForTopNode_old(HtmlNode node)
        {
            string result = "";

            switch (node.Name)
            {
                case "h1":
                case "H1":
                    result = string.Format("<Paragraph FontSize=\"30\">{0}</Paragraph>", CleanText(node.InnerText));
                    break;
                case "h2":
                case "H2":
                    result = string.Format("<Paragraph FontSize=\"24\">{0}</Paragraph>", CleanText(node.InnerText));
                    break;
                case "h3":
                case "H3":
                    result = string.Format("<Paragraph><Bold>{0}</Bold></Paragraph>", CleanText(node.InnerText));
                    break;
                case "ul":
                case "UL":
                    result = String.Format("<Paragraph Margin=\"20,0,0,0\"><LineBreak />{0}</Paragraph>", GenerateBlockForChildren(node));
                    break;
                default:
                    //var text = GenerateBlockForChildren(node);
                    //if (!string.IsNullOrEmpty(text))
                    //{
                    //    result = String.Format("<Paragraph>{0}</Paragraph>", text);
                    //}
                    //else
                    //{
                    //    result = "";
                    //}
                    result = String.Format("<Paragraph>{0}</Paragraph>", GenerateBlockForChildren(node));
                    break;
            }

            return result;
        }

        private static string GenerateBlockForNode_old(HtmlNode node)
        {
            string result = "";

            switch (node.Name)
            {
                case "div":
                    result = String.Format("<Span>{0}</Span>", GenerateBlockForChildren(node));
                    break;
                case "p":
                case "P":
                    result = String.Format("<Span><LineBreak />{0}<LineBreak /></Span>", GenerateBlockForChildren(node));
                    break;
                case "img":
                case "IMG":
                    {


                        //if (int.Parse(node.Attributes["width"].Value) > 500)
                        //    result = "<Span><InlineUIContainer><Image Style=\"{StaticResource RTImage}\" Width=\"500\" Source=\"" + node.Attributes["src"].Value + "\"></Image></InlineUIContainer></Span>";

                        //else
                        result = "<Span><InlineUIContainer><Image Style=\"{StaticResource RTImage}\" Source=\"" + node.Attributes["src"].Value + "\"></Image></InlineUIContainer></Span>";
                    }
                    break;
                case "a":
                case "A":
                    if (node.ChildNodes.Count == 1 && (node.FirstChild.Name == "img" || node.FirstChild.Name == "IMG"))
                        result = GenerateBlockForChildren(node);
                    else
                    {
                        if (node.ParentNode != null && (node.ParentNode.Name == "li" || node.ParentNode.Name == "LI"))
                            result = "<Span><InlineUIContainer><HyperlinkButton Style=\"{StaticResource RTLinkLI}\" NavigateUri=\"" + node.Attributes["href"].Value + "\">" + CleanText(node.InnerText) + "</HyperlinkButton></InlineUIContainer></Span>";
                        else if ((node.NextSibling == null || string.IsNullOrWhiteSpace(node.NextSibling.InnerText)) && (node.PreviousSibling == null || string.IsNullOrWhiteSpace(node.PreviousSibling.InnerText)))
                            result = "<Span><InlineUIContainer><HyperlinkButton Style=\"{StaticResource RTLinkOnly}\" NavigateUri=\"" + node.Attributes["href"].Value + "\">" + CleanText(node.InnerText) + "</HyperlinkButton></InlineUIContainer></Span>";
                        else
                            result = "<Span><InlineUIContainer><HyperlinkButton Style=\"{StaticResource RTLink}\" NavigateUri=\"" + node.Attributes["href"].Value + "\">" + CleanText(node.InnerText) + "</HyperlinkButton></InlineUIContainer></Span>";
                    }
                    break;
                case "li":
                case "LI":
                    result = "<Span><InlineUIContainer><Ellipse Style=\"{StaticResource RTBullet}\"/></InlineUIContainer>" + GenerateBlockForChildren(node) + "<LineBreak /></Span>";
                    break;
                case "b":
                case "B":
                    result = String.Format("<Bold>{0}</Bold>", GenerateBlockForChildren(node));
                    break;
                case "i":
                case "I":
                    result = String.Format("<Italic>{0}</Italic>", GenerateBlockForChildren(node));
                    break;
                case "u":
                case "U":
                    result = String.Format("<Underline>{0}</Underline>", GenerateBlockForChildren(node));
                    break;
                case "br":
                case "BR":
                    result = "<LineBreak />";
                    break;
                case "span":
                case "Span":
                    result = String.Format("<Span>{0}</Span>", GenerateBlockForChildren(node));
                    break;
                case "#text":
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        result = CleanText(node.InnerText);
                    else
                        result = "";
                    break;
                default:
                    result = CleanText(node.InnerText);
                    break;
            }

            return result;
        }

        private static string GenerateBlockForChildren(HtmlNode node)
        {
            string result = "";
            foreach (HtmlNode child in node.ChildNodes)
            {
                result += GenerateBlockForNode(child);
            }
            if (result == "")
                result = CleanText(node.InnerText);
            return result;
        }

        #endregion


    public static readonly DependencyProperty HTMLText = DependencyProperty.RegisterAttached(
        "HTMLText",
        typeof(string),
        typeof(CustomProperties),
        new PropertyMetadata(
            "Saraba1st</br>",
            OnHTMLTextChanged)
        );

    private static void OnHTMLTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        Windows.UI.Xaml.Controls.WebView wv = obj as Windows.UI.Xaml.Controls.WebView;
        if (wv != null)
        {
            wv.NavigateToString((string)args.NewValue);
        }
    }

    public static string GetHTMLText(DependencyObject obj)
    {
        return (string)obj.GetValue(HTMLText);
    }

    public static void SetHTMLText(DependencyObject obj, string value)
    {
        obj.SetValue(HTMLText, value);
    }

        //        //    public static readonly DependencyProperty HtmlProperty =
        //        //        DependencyProperty.RegisterAttached("Html", typeof(string), typeof(CustomProperties), new PropertyMetadata(null, HtmlChanged));

        //        //    public static void SetHtml(DependencyObject obj, string value)
        //        //    {
        //        //        obj.SetValue(HtmlProperty, value);
        //        //    }

        //        //    public static string GetHtml(DependencyObject obj)
        //        //    {
        //        //        return (string)obj.GetValue(HtmlProperty);
        //        //    }

        //        //    private static async void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //        //    {
        //        //        // Get the target RichTextBlock
        //        //        RichTextBlock richText = d as RichTextBlock;
        //        //        if (richText == null) return;

        //        //        // Wrap the value of the Html property in a div and convert it to a new RichTextBlock
        //        //        string xhtml = string.Format("<div>{0}</div>", e.NewValue as string);
        //        //        xhtml = xhtml.Replace("\r", "").Replace("\n", "<br />");
        //        //        RichTextBlock newRichText = null;
        //        //        if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
        //        //        {
        //        //            // In design mode we swallow all exceptions to make editing more friendly
        //        //            string xaml = "";
        //        //            try
        //        //            {
        //        //                xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
        //        //                newRichText = (RichTextBlock)XamlReader.Load(xaml);
        //        //            }
        //        //            catch (Exception ex)
        //        //            {
        //        //                string errorxaml = string.Format(@"
        //        //                    <RichTextBlock 
        //        //                     xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        //        //                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        //        //                    >
        //        //                        <Paragraph>An exception occurred while converting HTML to XAML: {0}</Paragraph>
        //        //                        <Paragraph />
        //        //                        <Paragraph>HTML:</Paragraph>
        //        //                        <Paragraph>{1}</Paragraph>
        //        //                        <Paragraph />
        //        //                        <Paragraph>XAML:</Paragraph>
        //        //                        <Paragraph>{2}</Paragraph>
        //        //                    </RichTextBlock>",
        //        //                    ex.Message,
        //        //                    EncodeXml(xhtml),
        //        //                    EncodeXml(xaml)
        //        //                );
        //        //                newRichText = (RichTextBlock)XamlReader.Load(errorxaml);
        //        //            } // Display a friendly error in design mode.
        //        //        }
        //        //        else
        //        //        {
        //        //            // When not in design mode, we let the application handle any exceptions
        //        //            string xaml = "";
        //        //            try
        //        //            {
        //        //                xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
        //        //                newRichText = (RichTextBlock)XamlReader.Load(xaml);
        //        //            }
        //        //            catch (Exception ex)
        //        //            {
        //        //                string errorxaml = string.Format(@"
        //        //                    <RichTextBlock 
        //        //                     xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        //        //                     xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        //        //                    >
        //        //                        <Paragraph>Cannot convert HTML to XAML. Please ensure that the HTML content is valid.</Paragraph>
        //        //                        <Paragraph />
        //        //                        <Paragraph>HTML:</Paragraph>
        //        //                        <Paragraph>{0}</Paragraph>
        //        //                    </RichTextBlock>",
        //        //                    EncodeXml(xhtml)
        //        //                );
        //        //                newRichText = (RichTextBlock)XamlReader.Load(errorxaml);
        //        //            } // Display a friendly error in design mode.
        //        //        }

        //        //        // Move the blocks in the new RichTextBlock to the target RichTextBlock
        //        //        richText.Blocks.Clear();
        //        //        if (newRichText != null)
        //        //        {
        //        //            for (int i = newRichText.Blocks.Count - 1; i >= 0; i--)
        //        //            {
        //        //                Block b = newRichText.Blocks[i];
        //        //                newRichText.Blocks.RemoveAt(i);
        //        //                richText.Blocks.Insert(0, b);
        //        //            }
        //        //        }
        //        //    }

        //        //    private static string EncodeXml(string xml)
        //        //    {
        //        //        string encodedXml = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        //        //        return encodedXml;
        //        //    }

        //        //    private static XsltProcessor Html2XamlProcessor;

        //        //    private static async Task<string> ConvertHtmlToXamlRichTextBlock(string xhtml)
        //        //    {
        //        //        // Load XHTML fragment as XML document
        //        //        XmlDocument xhtmlDoc = new XmlDocument();
        //        //        xhtmlDoc.LoadXml(xhtml);

        //        //        if (Html2XamlProcessor == null)
        //        //        {
        //        //            // Read XSLT. In design mode we cannot access the xslt from the file system (with Build Action = Content), 
        //        //            // so we use it as an embedded resource instead:
        //        //            Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //        //            Windows.Storage.StorageFile file = await folder.GetFileAsync(@"Assets\RichTextBlockHtml2Xaml.xslt");
        //        //            string content = await Windows.Storage.FileIO.ReadTextAsync(file);
        //        //            XmlDocument html2XamlXslDoc = new XmlDocument();
        //        //            html2XamlXslDoc.LoadXml(content);
        //        //            Html2XamlProcessor = new XsltProcessor(html2XamlXslDoc);
        //        //        }

        //        //        // Apply XSLT to XML
        //        //        string xaml = Html2XamlProcessor.TransformToString(xhtmlDoc.FirstChild);
        //        //        return xaml;
        //        //    }
    }
}
