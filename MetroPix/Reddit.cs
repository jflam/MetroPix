using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public class PhotoStream
    {
        public string Title { get; set; }
        public List<PhotoSummary> Photos { get; set; }

        public PhotoStream(string title, List<PhotoSummary> photos)
        {
            Title = title;
            Photos = photos;
        }
    }

    public abstract class BaseSiteParser
    {
        protected abstract void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos);

        public virtual async Task<PhotoStream> Parse(Uri uri)
        {
            var photos = new List<PhotoSummary>();
            var html = await NetworkManager.Current.GetStringAsync(uri);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var title = doc.DocumentNode.FindFirst((node) => node.Name.ToLower() == "title").InnerText;
            ProcessDocument(doc, photos);
            return new PhotoStream(title, photos);
        }
    }

    // Encapsulates knowledge of all known domain-specific parsers and
    // delegates parsing to the approrpriate implementation.
    public class UriDispatcher
    {
        private Dictionary<string, BaseSiteParser> _hostToParserMap = new Dictionary<string, BaseSiteParser>
        {
            { "www.boston.com", new BigPictureSiteParser() },
            { "boston.com", new BigPictureSiteParser() },
            { "www.flickr.com", new FlickrSiteParser() },
            { "flickr.com", new FlickrSiteParser() },
            { "imgur.com", new ImgurSiteParser() },
            { "www.500px.com", new FiveHunderedPixelsSiteParser() },
        };

        public async Task<PhotoStream> Parse(Uri uri)
        {
            var hostName = uri.Host.ToLower(); 
            BaseSiteParser parser = null;
            if (_hostToParserMap.TryGetValue(hostName, out parser))
            {
                return await parser.Parse(uri);
            }
            else
            {
                return null;
            }
        }
    }

    public class BigPictureSiteParser : BaseSiteParser
    {
        protected override void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos)
        {
            // Filter
            List<HtmlNode> imageBlocks = doc.DocumentNode.FindAll((currentNode) =>
            {
                var classAttr = currentNode.GetAttributeValue("class", String.Empty);
                return currentNode.Name.ToLower() == "div" && (classAttr == "bpImageTop" || classAttr == "bpBoth");
            });

            // Build
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
                photos.Add(photo);
            }
        }
    }

    public class FlickrSiteParser : BaseSiteParser
    {
        // TODO: need to have a pre-processor that does the right thing based on the
        // type of page that we're looking at. Right now this is a loos processdocument
        // that will do both Explore pages and Photostreams. However, things are quite
        // different structurally between the pages.
        protected override void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos)
        {
            // Filter
            List<HtmlNode> imageBlocks = doc.DocumentNode.FindAll((currentNode) =>
            {
                var classAttr = currentNode.GetAttributeValue("class", String.Empty);
                return currentNode.Name.ToLower() == "span" && classAttr.Contains("photo_container");
            });

            // Build
            foreach (var imageBlock in imageBlocks)
            {
                var caption = imageBlock.FindFirst((captionNode) => { return captionNode.Name.ToLower() == "a"; }).GetAttributeValue("title", String.Empty);
                var image = imageBlock.FindFirst((imageNode) => { return imageNode.Name.ToLower() == "img" && imageNode.GetAttributeValue("class", String.Empty) == "pc_img"; }).GetAttributeValue("src", String.Empty);
                // TODO - do better than this. We can decode the Flickr image url syntax 
                // _m is the medium sized image ... may want to replace with the _l image for large?
                image = image.Replace("_m", "_z");
                var photo = new PhotoSummary
                {
                    Author = "?",
                    Caption = caption,
                    PhotoUri = new Uri(image)
                };
                photos.Add(photo);
            }
        }
    }

    public class ImgurSiteParser : BaseSiteParser
    {
        private string CleanupCaption(string caption)
        {
            // Strip out entity references and HTML tags
            return Regex.Replace(Regex.Replace(caption, @"<(.|\n)*?>", String.Empty), @"&#\d+;", String.Empty);
        }

        protected override void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos)
        {
            // Filter
            List<HtmlNode> imageBlocks = doc.DocumentNode.FindAll((currentNode) =>
            {
                var classAttr = currentNode.GetAttributeValue("class", String.Empty);
                return currentNode.Name.ToLower() == "div" && classAttr == "post";
            });

            // Build
            foreach (var imageBlock in imageBlocks)
            {
                // The image URI can be computed from the image id
                var id = imageBlock.GetAttributeValue("id", String.Empty);
                var url = String.Format("http://i.imgur.com/{0}.jpg", id);
                var caption = imageBlock.FindFirst((imageNode) => { return imageNode.Name.ToLower() == "img"; }).GetAttributeValue("title", string.Empty);
                var photo = new PhotoSummary
                {
                    Author = "?",
                    Caption = CleanupCaption(caption),
                    PhotoUri = new Uri(url)
                };
                photos.Add(photo);
            }
        }
    }

    public class FiveHunderedPixelsSiteParser : BaseSiteParser
    {
        protected override void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos)
        {
            // Filter
            List<HtmlNode> imageBlocks = doc.DocumentNode.FindAll((currentNode) =>
            {
                var classAttr = currentNode.GetAttributeValue("class", String.Empty);
                return currentNode.Name.ToLower() == "div" && classAttr == "thumb";
            });

            // Build
            foreach (var imageBlock in imageBlocks)
            {
                // The image URI can be computed from the image id
                var caption = imageBlock.GetAttributeValue("title", String.Empty);
                var image = imageBlock.FindFirst((imageNode) => { return imageNode.Name.ToLower() == "img"; }).GetAttributeValue("src", String.Empty);

                // The full sized pictures are 4.jpg
                image = image.Replace("3.jpg", "4.jpg");

                var photo = new PhotoSummary
                {
                    Author = "?",
                    Caption = caption,
                    PhotoUri = new Uri(image)
                };
                photos.Add(photo);
            }
        }
    }
    #region TODO Later

    // TODO: revive this as a brand new class since the BaseImageImporter related types are really just HTML screen
    // scraping types which certainly may be called from an RSS feed importer, but perhaps not?
    //public class RssImporter : BaseImageImporter
    //{
    //    public override async Task<List<PhotoSummary>> Parse(Uri uri)
    //    {
    //        var photos = new List<PhotoSummary>();
    //        var client = new SyndicationClient();
    //        var feed = await client.RetrieveFeedAsync(uri);
    //        foreach (var item in feed.Items)
    //        {
    //            var caption = item.Title.Text;
    //            var html = item.Summary.Text;
    //            var doc = new HtmlDocument();
    //            doc.LoadHtml(html);
    //            List<HtmlNode> imageUris = ExtractImagesFromNode(doc.DocumentNode);
    //            foreach (var imageUri in imageUris)
    //            {
    //                var photo = new PhotoSummary
    //                {
    //                    Author = "todo",
    //                    Caption = caption,
    //                    PhotoUri = new Uri(imageUri.GetAttributeValue("src", String.Empty))
    //                };
    //                photos.Add(photo);
    //            }
    //        }
    //        return photos;
    //    }
    //}

    // TODO: rewrite this
    public class RedditSiteParser : BaseSiteParser
    {
        // TODO: make this use extension methods FindAll and friends
        protected override void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos)
        {
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
                                            photos.Add(photo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}