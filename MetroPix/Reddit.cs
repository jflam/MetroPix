using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.Web.Syndication;

namespace MetroPix
{
    public static class HtmlDocumentExtensionMethods
    {
        public static List<HtmlNode> FindAll(this HtmlNode node, Func<HtmlNode, bool> predicate) 
        {
            var result = new List<HtmlNode>();
            foreach (var element in node.DescendantNodes())
            {
                if (predicate(element))
                {
                    result.Add(element);
                }
            }
            return result;
        }

        public static HtmlNode FindFirst(this HtmlNode node, Func<HtmlNode, bool> predicate)
        {
            foreach (var element in node.DescendantNodes())
            {
                if (predicate(element))
                {
                    return element;
                }
            }
            return null;
        }
    }

    public class ImageImporter<T> where T: new()
    {
        protected List<PhotoSummary> _photos;

        public List<PhotoSummary> LastQuery
        {
            get
            {
                return _photos;
            }
        }

        private static T _singleton = new T();

        public static T Site
        {
            get { return _singleton; }
        }

        protected List<HtmlNode> ExtractImagesFromNode(HtmlNode node)
        {
            return node.FindAll((currentNode) =>
            {
                if (currentNode.Name.ToLower() == "img")
                {
                    // Get the Uri and look for the pattern
                    string src = currentNode.GetAttributeValue("src", String.Empty);
                    if (!String.IsNullOrEmpty(src))
                    {
                        // TODO: generalized pattern match -- can parameterize?
                        if (src.Contains("inapcache.boston.com"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
        }
    }

    public class HtmlImporter : ImageImporter<HtmlImporter>
    {
        public async Task<List<PhotoSummary>> Query(Uri uri)
        {
            _photos = new List<PhotoSummary>();
            var html = await NetworkManager.Current.GetStringAsync(uri);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Find all of the divs that contain the images
            List<HtmlNode> imageBlocks = doc.DocumentNode.FindAll((currentNode) =>
            {
                var classAttr = currentNode.GetAttributeValue("class", String.Empty);
                return currentNode.Name.ToLower() == "div" && (classAttr == "bpImageTop" || classAttr == "bpBoth");
            });

            foreach (var imageBlock in imageBlocks)
            {
                var caption = imageBlock.FindFirst((captionNode) => { return captionNode.Name.ToLower() == "div" && captionNode.GetAttributeValue("class", String.Empty) == "bpCaption"; }).InnerText;
                var image = imageBlock.FindFirst((imageNode) => { return imageNode.Name.ToLower() == "img" && imageNode.GetAttributeValue("src", String.Empty).Contains("inapcache.boston.com"); }).GetAttributeValue("src", String.Empty);
                var photo = new PhotoSummary
                {
                    Author = "?",
                    Caption = caption,
                    PhotoUri = new Uri(image)
                };
                _photos.Add(photo);                
            }

            return _photos;
        }
    }

    public class RssImporter : ImageImporter<RssImporter>
    {
        public async Task<List<PhotoSummary>> Query(Uri uri)
        {
            _photos = new List<PhotoSummary>();
            var client = new SyndicationClient();
            var feed = await client.RetrieveFeedAsync(uri);
            foreach (var item in feed.Items)
            {
                var caption = item.Title.Text;
                var html = item.Summary.Text;
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                List<HtmlNode> imageUris = ExtractImagesFromNode(doc.DocumentNode);
                foreach (var imageUri in imageUris)
                {
                    var photo = new PhotoSummary
                    {
                        Author = "todo",
                        Caption = caption,
                        PhotoUri = new Uri(imageUri.GetAttributeValue("src", String.Empty))
                    };
                    _photos.Add(photo);
                }
            }
            return _photos;
        }
    }

    public class ImgurImporter : ImageImporter<ImgurImporter>
    {
        private List<PhotoSummary> Parse(string html)
        {
            var result = new List<PhotoSummary>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            foreach (var element in doc.DocumentNode.DescendantNodes())
            {
                if (element.Name == "img")
                {
                    var href = element.GetAttributeValue("src", String.Empty);
                    if (!String.IsNullOrEmpty(href))
                    {
                        // TODO: bug in html agility pack in parsing attribute names with dashes: original-title becomes title
                        var title = element.GetAttributeValue("title", string.Empty);
                        var attrs = element.Attributes;
                        var index = href.LastIndexOf("b.jpg");
                        if (index > 0)
                        {
                            var photoUri = new Uri(href.Substring(0, index) + ".jpg");
                            var photo = new PhotoSummary
                            {
                                Author = "todo",
                                PhotoUri = photoUri,
                                Caption = title
                            };
                            result.Add(photo);
                        }
                    }
                }
            }
            return result;
        }

        public async Task<List<PhotoSummary>> Query(Uri uri)
        {
            var html = await NetworkManager.Current.GetStringAsync(uri);
            _photos = Parse(html);
            return _photos;
        }
    }

    public class RedditImporter : ImageImporter<RedditImporter>
    {
        private List<PhotoSummary> Parse(string html)
        {
            var result = new List<PhotoSummary>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var linksTable = doc.GetElementbyId("siteTable");
            if (linksTable != null)
            {
                foreach (var item in linksTable.ChildNodes)
                {
                    if (item.GetAttributeValue("class", String.Empty) != "clearleft")
                    {
                        foreach (var element in item.DescendantNodes())
                        {
                            if (element.Name == "a")
                            {
                                var klass = element.GetAttributeValue("class", String.Empty).Trim();
                                if (klass == "title")
                                {
                                    var href = element.GetAttributeValue("href", String.Empty);
                                    if (!String.IsNullOrEmpty(href))
                                    {
                                        var uri = new Uri(href);
                                        if (uri.Host == "imgur.com")
                                        {
                                            // We just need to programatically transform uri to get the photo Uri
                                            var photoUrl = String.Format("{0}://i.{1}{2}.jpg", uri.Scheme, uri.Host, uri.LocalPath);
                                            var photoUri = new Uri(photoUrl);
                                            var photo = new PhotoSummary
                                            {
                                                PhotoUri = photoUri,
                                                Caption = element.InnerText,
                                                Author = "todo"
                                            };
                                            result.Add(photo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Point to a reddit and extract the picture urls
        public async Task<List<PhotoSummary>> Query(Uri uri)
        {
            var html = await NetworkManager.Current.GetStringAsync(uri);
            _photos = Parse(html);
            return _photos;
        }
    }
}