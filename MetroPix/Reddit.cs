using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.Web.Syndication;

namespace MetroPix
{
    public static class HtmlDocumentExtensionMethods
    {
        public static List<Uri> FindAll(this HtmlDocument doc, Func<HtmlNode, Uri> predicate) 
        {
            var result = new List<Uri>();
            foreach (var element in doc.DocumentNode.DescendantNodes())
            {
                Uri imageUri = predicate(element);
                if (imageUri != null)
                {
                    result.Add(imageUri);
                }
            }
            return result;
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

        protected List<Uri> ExtractImagesFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.FindAll((node) =>
            {
                if (node.Name.ToLower() == "img")
                {
                    // Get the Uri and look for the pattern
                    string src = node.GetAttributeValue("src", String.Empty);
                    if (!String.IsNullOrEmpty(src))
                    {
                        // TODO: generalized pattern match -- can parameterize?
                        if (src.Contains("inapcache.boston.com"))
                        {
                            return new Uri(src);
                        }
                    }
                }
                return null;
            });
        }
    }

    public class HtmlImporter : ImageImporter<HtmlImporter>
    {
        public async Task<List<PhotoSummary>> Query(Uri uri)
        {
            _photos = new List<PhotoSummary>();
            var html = await NetworkManager.Current.GetStringAsync(uri);
            List<Uri> imageUris = ExtractImagesFromHtml(html);
            foreach (var imageUri in imageUris)
            {
                var photo = new PhotoSummary
                {
                    Caption = "Unknown",
                    Author = "Unknown",
                    PhotoUri = imageUri
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
                List<Uri> imageUris = ExtractImagesFromHtml(html);
                foreach (Uri imageUri in imageUris)
                {
                    var photo = new PhotoSummary
                    {
                        Author = "todo",
                        Caption = caption,
                        PhotoUri = imageUri
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