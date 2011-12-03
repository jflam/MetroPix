using System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class FrontPage : Page
    {
        public FrontPage()
        {
            InitializeComponent();
        }

        private async void LoadSomePhotos()
        {
            // TODO: do some caching of the photo data since back is likely to be pretty popular
            var obj = await FiveHundredPixels.Site.GetPhotos("editors");
            var photos = obj["photos"].GetArray();
            var count = photos.Count;
            for (uint i = 0; i < count; i++)
            {
                var photo = photos.GetObjectAt(i);
                int id = Convert.ToInt32(photo["id"].GetNumber());
                var photoUri = FiveHundredPixels.Site.GetPhotoUri(photo, 4);

                // Make me an image that contains an overlay
                var grid = new Grid();

                var image = new Image();
                image.Margin = new Thickness(5, 0, 5, 0);
                image.Source = new BitmapImage(photoUri);
                image.Height = 650;
                image.Tapped += (sender, args) =>
                {
                    // BUGBUG: all event handlers are not wired up when I go back!
                    Frame.Navigate(typeof(SinglePicture).FullName, id);
                };

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
                text.FontWeight = FontWeights.Bold;
                text.Foreground = new SolidColorBrush(Colors.White);
                text.FontSize = 24;
                text.Text = photo["name"].GetString();

                var name = new TextBlock();
                name.Margin = new Thickness(10, 0, 0, 0);
                name.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
                name.Foreground = new SolidColorBrush(Colors.White);                
                name.FontSize = 12;
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
            LoadSomePhotos();
        }
    }
}
