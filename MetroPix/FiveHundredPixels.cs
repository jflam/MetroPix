using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroPix
{
    public class PhotoSummary
    {
        public int Id { get; set; }
        //public BitmapImage Photo { get; set; }
        public Uri PhotoUri { get; set; }
        public string Caption { get; set; }
        public string Author { get; set; }
        public int Votes { get; set; }
        public double Rating { get; set; }
    }

    public class PhotoDetails
    {
        public Uri PhotoUri { get; set; }
        public string Caption { get; set; }
        public string Artist { get; set; }
        public string UserName { get; set; }
        public double Rating { get; set; }
        public int Views { get; set; }
        public int Votes { get; set; }
        public int Favs { get; set; }
    }

    public class FiveHundredPixels
    {
        private HttpClient _client = new HttpClient();

        // Secret keys
        private const string CONSUMER_KEY = "YNkYLEDEc7bDaIW3JzJfEKO5fOmzvZr2QYvNJ1ti";

        // This API returns details of a single photo, including optional comments
        private const string GET_PHOTO_API = "https://api.500px.com/v1/photos/{0}?image_size={1}&consumer_key={2}";

        // This API returns square thumbnails of the photos
        // I can probably use these as placeholder images scaled appropriately?
        private const string GET_PHOTOS_API = "https://api.500px.com/v1/photos?feature={0}&page={1}&consumer_key={2}&rpp={3}";

        private const string GET_USER_PHOTOS_API = "https://api.500px.com/v1/photos?feature={0}&username={1}&page={2}&consumer_key={3}&rpp={4}";

        private FiveHundredPixels() { }

        private Uri GetPhotoUri(string uri, int size)
        {
            var directory = uri.Substring(0, uri.LastIndexOf('/'));
            var photoUri = String.Format("{0}/{1}.jpg", directory, size);
            return new Uri(photoUri);
        }

        private async Task<string> SubmitRequest(string url)
        {
            int i = 0;
            while (true)
            {
                try
                {
                    return await _client.GetStringAsync(url);
                }
                catch (Exception e)
                {
                    i++;
                    if (i < 5)
                    {
                        continue;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        private List<PhotoSummary> _photos;


        // TODO: fix how we think about querying for the photos
        // - Need to have a sorted list of pictures
        // - Once we have the first picture loaded we add it to the UI
        // - Once we have the next picture loaded we add it to the UI
        // - Need to have a mechanism to correlate the index of the request with the photo
        // - When that photo == current index we append to the list
        // - Do we need to fire events from here when we get a new photo in the sequence?
        // - We hide the out of order nature from the caller?
        // - What happens with a timeout?

        // TODO: paging
        public async Task<List<PhotoSummary>> Query(string collection, int count = 20, int size = 4)
        {
            // If collection is user:<user_name> or user_friends:<user_name> or user_favorites:<user_name>
            // we need to use the user query
            var index = collection.IndexOf(':');
            string requestUri;
            if (index >= 0)
            {
                var userCollection = collection.Substring(0, index);
                var userName = collection.Substring(index + 1);
                requestUri = String.Format(GET_USER_PHOTOS_API, userCollection, userName, 1, CONSUMER_KEY, count); 
            }
            else
            {
                requestUri = String.Format(GET_PHOTOS_API, collection, 1, CONSUMER_KEY, count);
            }

            var json = await SubmitRequest(requestUri);
            var photos = JsonObject.Parse(json)["photos"].GetArray();
            var result = new List<PhotoSummary>();
            for (uint i = 0; i < photos.Count; i++)
            {
                var photo = photos.GetObjectAt(i);

                int id = Convert.ToInt32(photo["id"].GetNumber());
                string caption = photo["name"].GetString();
                string author = photo["user"].GetObject()["fullname"].GetString();
                double rating = photo["rating"].GetNumber();
                int votes = Convert.ToInt32(photo["rating"].GetNumber());
                Uri uri = GetPhotoUri(photo["image_url"].GetString(), size);
                //BitmapImage bitmap = await DownloadImageAsync(uri);

                result.Add(new PhotoSummary
                {
                    Id = id,
                    //Photo = bitmap,
                    Caption = caption,
                    Author = author,
                    PhotoUri = uri,
                    Rating = rating,
                    Votes = votes
                });
            }
            _photos = result;
            return result;
        }

        public async Task<PhotoDetails> GetFullSizePhoto(int id)
        {
            var requestUri = String.Format(GET_PHOTO_API, id, 4, CONSUMER_KEY);
            var json = await SubmitRequest(requestUri);
            var photo = JsonObject.Parse(json);
            var result = new PhotoDetails
            {
                PhotoUri = new Uri(photo["photo"].GetObject()["image_url"].GetString()),
                Caption = photo["photo"].GetObject()["name"].GetString(),
                Artist = photo["photo"].GetObject()["user"].GetObject()["fullname"].GetString(),
                UserName = photo["photo"].GetObject()["user"].GetObject()["username"].GetString(),
                Rating = photo["photo"].GetObject()["rating"].GetNumber(),
                Views = Convert.ToInt32(photo["photo"].GetObject()["times_viewed"].GetNumber()),
                Votes = Convert.ToInt32(photo["photo"].GetObject()["votes_count"].GetNumber()),
                Favs = Convert.ToInt32(photo["photo"].GetObject()["favorites_count"].GetNumber())
            };
            return result;
        }

        // TODO: Place to store front page state ... need to move this somewhere else
        public double ScrollOffset { get; set; }

        public List<PhotoSummary> LastQuery
        {
            get
            {
                return _photos;
            }
        }

        private static FiveHundredPixels _singleton = new FiveHundredPixels();

        public static FiveHundredPixels Site
        {
            get { return _singleton; }
        }
    }

    public class Experiments
    {
        public Task<BitmapImage> DownloadBitmapImageAsync(Uri uri)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();
            var image = new BitmapImage(uri);
            image.ImageOpened += (sender, args) =>
            {
                tcs.SetResult(image);
            };
            return tcs.Task;
        }

        //public async Task<IEnumerable<BitmapImage>> LoadImages(List<Uri> uris)
        //{

        //}
    }
}