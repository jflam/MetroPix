using System;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            InitializeComponent();
            LoadSomePhotos();
        }

        // Secret keys
        private const string CONSUMER_KEY = "YNkYLEDEc7bDaIW3JzJfEKO5fOmzvZr2QYvNJ1ti";

        // This API returns details of a single photo, including optional comments
        private const string GET_PHOTO_API = "https://api.500px.com/v1/photos/{0}?image_size={1}&consumer_key={2}";

        // This API returns square thumbnails of the photos
        // I can probably use these as placeholder images scaled appropriately?
        private const string GET_PHOTOS_API = "https://api.500px.com/v1/photos?feature={0}&page={1}&consumer_key={2}";

        private async void LoadSomePhotos()
        {
            var requestUri = String.Format(GET_PHOTOS_API, "editors", 1, CONSUMER_KEY);
            var client = new HttpClient();
            var json = await client.GetStringAsync(requestUri);
            var obj = JsonObject.Parse(json);
            var photos = obj["photos"].GetArray();
            var count = photos.Count;
            for (uint i = 0; i < count; i++)
            {
                var photo = photos.GetObjectAt(i);
                double id = photo["id"].GetNumber();
                string thumbUrl = photo["image_url"].GetString();
                
                requestUri = String.Format(GET_PHOTO_API, id, 4, CONSUMER_KEY);
                json = await client.GetStringAsync(requestUri);

                var photoDetails = JsonObject.Parse(json)["photo"].GetObject();
                var photoUrl = photoDetails["image_url"].GetString();

                // Make me an image that contains an overlay
                var grid = new Grid();

                var image = new Image();
                image.Margin = new Thickness(5, 0, 5, 0);
                image.Source = new BitmapImage(new Uri(photoUrl));
                image.Height = 550;

                // Make a text block with the caption
                var canvas = new Canvas();
                canvas.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                canvas.Width = image.Width;
                canvas.Margin = new Thickness(5, 0, 5, 0);
                canvas.Height = 60;
                canvas.Background = new SolidColorBrush(Colors.Black);
                canvas.Opacity = 0.8;

                var text = new TextBlock();
                text.Margin = new Thickness(10, 0, 0, 0);
                text.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
                text.Foreground = new SolidColorBrush(Colors.White);
                text.FontSize = 24;
                text.Text = photo["name"].GetString();

                var name = new TextBlock();
                name.Margin = new Thickness(10, 0, 0, 0);
                name.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
                name.Foreground = new SolidColorBrush(Colors.White);
                name.FontSize = 14;
                name.Text = photo["user"].GetObject()["fullname"].GetString();

                var left = new StackPanel();
                left.Orientation = Orientation.Vertical;
                left.Children.Add(text);
                left.Children.Add(name);

                canvas.Children.Add(left);

                grid.Children.Add(image);
                grid.Children.Add(canvas);

                Photos.Children.Add(grid);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
