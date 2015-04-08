using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using Microsoft.Phone.Controls;
using CodeKicker.BBCode;
using CaledosLab.Portable.Logging;

namespace Facepunch8.Utilities
{
    public class PostConverter : IValueConverter
    {
        private BBCodeParser _parser;

        public PostConverter()
        {
            #region Parser
            Func<IAttributeRenderingContext, string> fixUpFunction =
                (x) =>
                {
                    return x.AttributeValue == null ? "" : x.AttributeValue.Split(';')[0];
                };
            Func<IAttributeRenderingContext, string> videoStuff =
                (x) =>
                {
                    return x.AttributeValue == null ? "" : x.AttributeValue.Split(';')[0];
                };

            Func<string, string> urlStuff =
                (x) =>
                {
                    // Can't have inline... how should I fix this?
                    if (!x.Contains("InlineUIContainer") && !x.Contains("Hyperlink") && !x.Contains("LineBreak"))
                        return "<Hyperlink NavigateUri=\"" + x + "\" TargetName=\"_blank\">" + x + "</Hyperlink>";
                    else if (x.Contains("InlineUIContainer"))
                        return "<Hyperlink NavigateUri=\"" + x.Split('"')[1] + "\" TargetName=\"_blank\">Embedded URL</Hyperlink><LineBreak/>" + "<InlineUIContainer><Image Source=\"" + x.Split('"')[1] + "\" /></InlineUIContainer>";
                    else if (x.Contains("Hyperlink"))
                        return "<Hyperlink NavigateUri=\"" + x.Split('"')[1] + "\" TargetName=\"_blank\">Embedded URL</Hyperlink><LineBreak/>" + "<Hyperlink NavigateUri=\"" + x.Split('"')[1] + "\" TargetName=\"_blank\">" + x.Split('"')[1] + "</Hyperlink><LineBreak/>";
                    else
                        return "URL</Hyperlink>";
                };
            var endRTB = "</Paragraph></RichTextBox>";
            var openRTB = "<RichTextBox IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>";

            //Image display settings.
            BBTag img, t, imgT;
            if (Settings.DisplayImages)
            {
                /*<Image> 
                      <Image.Source> 
                        <BitmapImage UriSource="{Binding ImgUrl}" CreateOptions="BackgroundCreation"/> 
                      </Image.Source> 
                    </Image>*/
                img = new BBTag("img", "<InlineUIContainer><Image Source=\"${content}\" Width=\"Auto\" MaxWidth=\"480\" /></InlineUIContainer>", "", false, true);
                t = new BBTag("t", "<InlineUIContainer><Image Source=\"${content}\" MaxWidth=\"150\" /></InlineUIContainer>", "", false, true);
                imgT = new BBTag("img_thumb", "<InlineUIContainer><Image Source=\"${content}\" MaxWidth=\"150\" /></InlineUIContainer>", "", false, true);
            }
            else
            {
                /* Doesn't work if img was within a hyperlink... */
                img = new BBTag("img", "<Hyperlink NavigateUri=\"${content}\" TargetName=\"_blank\">${content}</Hyperlink><LineBreak/>", "", false, true);
                t = new BBTag("t", "<Hyperlink NavigateUri=\"${content}\" TargetName=\"_blank\">${content}</Hyperlink><LineBreak/>", "", false, true);
                imgT = new BBTag("img_thumb", "", "<Hyperlink NavigateUri=\"${content}\" TargetName=\"_blank\">${content}</Hyperlink><LineBreak/>", false, true);
            }

            var quote = endRTB +
                           "<RichTextBox Padding=\"3,0,0,0\" Margin=\"15,0,0,0\" Background=\"#aaccee\" Foreground=\"#0055aa\" BorderBrush=\"#aaccee\" BorderThickness=\"2\" IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>${user} posted:</Paragraph></RichTextBox>" +
                           "<RichTextBox Padding=\"4,0,3,3\" Margin=\"15,0,0,0\" Background=\"#bbddff\" Foreground=\"Black\" BorderBrush=\"#aaccee\" BorderThickness=\"2\" IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>";
            var endQuote = endRTB + openRTB;

            if ((Settings.CurrentTheme == Settings.Theme.Dark) || (Settings.CurrentTheme == Settings.Theme.System && !App.IsLightTheme))
            {
                // also helps with long posts...
                quote = endRTB +
                          "<RichTextBox Padding=\"3,0,0,0\" Margin=\"15,0,0,0\" Background=\"#444444\" Foreground=\"White\" BorderBrush=\"#444444\" BorderThickness=\"2\" IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>${user} posted:</Paragraph></RichTextBox>" +
                          "<RichTextBox Padding=\"4,0,3,3\" Margin=\"15,0,0,0\" Background=\"#686868\" Foreground=\"White\" BorderBrush=\"#444444\" BorderThickness=\"2\" IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>";
            }

            _parser = new BBCodeParser(new[]
            {
                //fix urls: http://bbcode.codeplex.com/workitem/9530
                new BBTag("url", "", "", true, true, urlStuff,
                    new BBAttribute("href", "", GetUrlTagHrefAttributeValue),
                    new BBAttribute("href", "href", GetUrlTagHrefAttributeValue)),
                new BBTag("b", "<Bold>", "</Bold>"), 
                new BBTag("h2", "<Span FontSize=\"32\">", "</Span>"),

                //Doesn't appear to work:
                /*new BBTag("del", "<span style='text-decoration:line-through'>", "</span>"), */

                new BBTag("i", "<Italic>", "</Italic>"), 
                new BBTag("u", "<Underline>", "</Underline>"), 
                new BBTag("editline", "<Underline>Edited on ", "</Underline>"), 
                new BBTag("highlight", "<Span Foreground=\"#417394\">", "</Span>"),
                //new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"), 
                img,
                t,
                imgT,
                //Highlights that it's a quote with color...
                new BBTag("quote", quote, endQuote, new BBAttribute("user", "", fixUpFunction, HtmlEncodingMode.UnsafeDontEncode), new BBAttribute("user", "user")),
                //new BBTag("quote", "<Span Foreground=\"#417394\"><Bold>${user}</Bold><LineBreak/>", "</Span><LineBreak/>", new BBAttribute("user", "", fixUpFunction, HtmlEncodingMode.UnsafeDontEncode), new BBAttribute("user", "user")),
                /*new BBTag("list", "<ul>", "</ul>"), 
                new BBTag("*", "<li>", "</li>", true, false), */

                new BBTag("video", "<Hyperlink NavigateUri=\"${content}\"  TargetName=\"_blank\">", "</Hyperlink>", true, false, new BBAttribute("href", "", videoFuck), new BBAttribute("href", "href")), 
                new BBTag("media", "<Hyperlink NavigateUri=\"${content}\" TargetName=\"_blank\">${content}</Hyperlink>", "", false, true), 
                new BBTag("hd", "<Hyperlink NavigateUri=\"${content}\" TargetName=\"_blank\">${content}</Hyperlink>", "", false, true),
                //what else...?
            });
            #endregion
        }

        static string GetUrlTagHrefAttributeValue(IAttributeRenderingContext attributeRenderingContext)
        {
            if (!string.IsNullOrWhiteSpace(attributeRenderingContext.AttributeValue))
                return attributeRenderingContext.AttributeValue.Replace("&quot;", ""); //explicit href attribute on url-Tag

            var tagContent = attributeRenderingContext.GetAttributeValueByID(BBTag.ContentPlaceholderName);
            return tagContent.Replace("&quot;", "");
        }

        ///<summary>
        ///Converter class used to evaluate and highlight context string results
        ///</summary>
        ///
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value.ToString();
            str = System.Net.HttpUtility.HtmlEncode(str.Replace("\r", "")).Replace("\n", "<LineBreak/>");

            string bbcode = _parser.ToHtml(str);

            try
            {
                string xaml = "<StackPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Orientation=\"Vertical\"><RichTextBox IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>" + bbcode + "</Paragraph></RichTextBox></StackPanel>";
                var obj = XamlReader.Load(xaml.Replace("", "'")); //&#146; (aka a ' character. weird)
                return obj;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e);
                string erroredXaml = "<RichTextBox IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>" + bbcode + "</Paragraph></RichTextBox>";
                string xaml = "<RichTextBox IsReadOnly=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ><Paragraph>An error has occurred:<LineBreak/>" + System.Net.HttpUtility.HtmlEncode(e.Message) + "</Paragraph></RichTextBox>";
                return XamlReader.Load(xaml);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
