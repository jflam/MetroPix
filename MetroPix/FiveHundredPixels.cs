using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace MetroPix
{
    public class FiveHundredPixels
    {
        // Secret keys
        private const string CONSUMER_KEY = "YNkYLEDEc7bDaIW3JzJfEKO5fOmzvZr2QYvNJ1ti";

        // This API returns details of a single photo, including optional comments
        private const string GET_PHOTO_API = "https://api.500px.com/v1/photos/{0}?image_size={1}&consumer_key={2}";

        // This API returns square thumbnails of the photos
        // I can probably use these as placeholder images scaled appropriately?
        private const string GET_PHOTOS_API = "https://api.500px.com/v1/photos?feature={0}&page={1}&consumer_key={2}";

        private HttpClient _client = new HttpClient();

        private FiveHundredPixels() { }

        public async Task<JsonObject> GetFullSizePhoto(int id)
        {
            var requestUri = String.Format(GET_PHOTO_API, id, 4, CONSUMER_KEY);
            var json = await _client.GetStringAsync(requestUri);
            return JsonObject.Parse(json);
        }

        // TODO: parameterize some more
        public async Task<JsonObject> GetPhotos(string collection)
        {
            var requestUri = String.Format(GET_PHOTOS_API, collection, 1, CONSUMER_KEY);
            var json = await _client.GetStringAsync(requestUri);
            return JsonObject.Parse(json);
        }

        public Uri GetPhotoUri(JsonObject photo, int size) 
        {
            var url = photo["image_url"].GetString();
            
            // Strip off everything after the last slash - that is the size of the file
            var directory = url.Substring(0, url.LastIndexOf('/'));
            var photoUri = String.Format("{0}/{1}.jpg", directory, size);
            return new Uri(photoUri);
        }

        private static FiveHundredPixels _singleton = new FiveHundredPixels();

        public static FiveHundredPixels Site
        {
            get { return _singleton; }
        }
    }
}