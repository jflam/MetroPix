using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

// Import pictures from Reddit. Uses Html Agility Pack mod.

namespace MetroPix
{
    public class ImageImporter
    {
        protected List<PhotoSummary> _photos;

        public List<PhotoSummary> LastQuery
        {
            get
            {
                return _photos;
            }
        }
    }

    public class ImgurImporter : ImageImporter
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

        private static ImgurImporter _singleton = new ImgurImporter();

        public static ImgurImporter Site
        {
            get { return _singleton; }
        }
    }

    public class RedditImporter : ImageImporter
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

        private static RedditImporter _singleton = new RedditImporter();

        public static RedditImporter Site
        {
            get { return _singleton; }
        }
    }
}