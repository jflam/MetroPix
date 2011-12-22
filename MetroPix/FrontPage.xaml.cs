using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class FrontPage : Page
    {
        public FrontPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadPhotos(RssImporter.Site.LastQuery);
            //LoadPhotos(ImgurImporter.Site.LastQuery);
            //LoadPhotos(RedditImporter.Site.LastQuery);
            //LoadPhotos(FiveHundredPixels.Site.LastQuery);
        }

        private async Task<Grid> RenderPhotoWithCaption(PhotoSummary photo, int index)
        {
            var image = new Image
            {
                Margin = new Thickness(5, 0, 5, 0),
                Source = await photo.GetPhotoAsync(),
                Height = 650,
                Tag = index
            };

            var text = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 24,
                Text = photo.Caption
            };

            var name = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI"),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12,
                Text = photo.Author
            };

            var left = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children = { text, name }
            };

            var canvas = new Canvas
            {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom,
                Width = image.Width,
                Margin = new Thickness(5, 0, 5, 0),
                Height = 60,
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.8,
                Children = { left }
            };

            var grid = new Grid
            {
                Children = { image, canvas }
            };
            return grid;
        }

        private async void LoadPhotos(List<PhotoSummary> photos)
        {
            for (int i = 0; i < photos.Count; i++)
            {
                Photos.Children.Add(await RenderPhotoWithCaption(photos[i], i));
            }
        }

        private void ScrollViewer_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            var image = e.OriginalSource as Image;
            var obj = e.OriginalSource;
            if (image != null)
            {
                if (image.Tag != null)
                {
                    FiveHundredPixels.Site.ScrollOffset = Viewer.HorizontalOffset;
                    Frame.Navigate(typeof(SinglePhotoPage), image.Tag);
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Home button
            var photos = await MetroPix.FiveHundredPixels.Site.Query("editors", 50, 4);
            Photos.Children.Clear();
            LoadPhotos(photos);
        }

        private void Viewer_Loaded_1(object sender, RoutedEventArgs e)
        {
            Viewer.ScrollToHorizontalOffset(FiveHundredPixels.Site.ScrollOffset);
        }
    }
}