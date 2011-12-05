using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace MetroPix
{
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

        private FiveHundredPixels() { }

        public async Task<JsonObject> GetFullSizePhoto(int id)
        {
            var requestUri = String.Format(GET_PHOTO_API, id, 4, CONSUMER_KEY);
            var json = await _client.GetStringAsync(requestUri);
            return JsonObject.Parse(json);
        }

        public async Task<List<PhotoSummary>> Query(string collection, int count = 20)
        {
            var requestUri = String.Format(GET_PHOTOS_API, collection, 1, CONSUMER_KEY, count);
            var json = await _client.GetStringAsync(requestUri);
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
                Uri uri = new Uri(photo["image_url"].GetString());

                var item = new PhotoSummary()
                {
                    Id = id,
                    Author = author,
                    Caption = caption,
                    Rating = rating,
                    Votes = votes,
                    PhotoUri = uri,
                };
                result.Add(item);
            }
            return result;
        }

        public Uri GetPhotoUri(PhotoSummary photo, int size)
        {
            var url = photo.PhotoUri.ToString();
            var directory = url.Substring(0, url.LastIndexOf('/'));
            var photoUri = String.Format("{0}/{1}.jpg", directory, size);
            return new Uri(photoUri);
        }

        // BUGBUG: I have to create a singleton FiveHundredPixels object because I can't do an HTTP request
        // via a second HttpClient instance in the same app!
        private static FiveHundredPixels _singleton = new FiveHundredPixels();

        public static FiveHundredPixels Site
        {
            get { return _singleton; }
        }
    }
}