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

    public abstract class BaseImageImporter
    {
        protected abstract void ProcessDocument(HtmlDocument doc, List<PhotoSummary> photos);

        public virtual async Task<List<PhotoSummary>> Parse(Uri uri)
        {
            var photos = new List<PhotoSummary>();
            var html = await NetworkManager.Current.GetStringAsync(uri);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            ProcessDocument(doc, photos);
            return photos;
        }
    }

    // Encapsulates knowledge of all known domain-specific parsers and
    // delegates parsing to the approrpriate implementation.
    public class UriDispatcher
    {
        private Dictionary<string, BaseImageImporter> _hostToParserMap = new Dictionary<string, BaseImageImporter>
        {
            { "www.boston.com", new BigPictureImporter() },
            { "boston.com", new BigPictureImporter() },
            { "www.flickr.com", new FlickrImporter() },
            { "flickr.com", new FlickrImporter() },
            { "imgur.com", new ImgurImporter() },
        };

        public async Task<List<PhotoSummary>> Parse(Uri uri)
        {
            var hostName = uri.Host.ToLower(); 
            BaseImageImporter parser = null;
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

    public class BigPictureImporter : BaseImageImporter
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

    public class FlickrImporter : BaseImageImporter
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

    public class ImgurImporter : BaseImageImporter
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
                var caption = imageBlock.FindFirst((imageNode) => { return imageNode.Name.ToLower() == "img"; }).GetAttributeValue("title", String.Empty);
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

    public class RedditImporter : BaseImageImporter
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
}