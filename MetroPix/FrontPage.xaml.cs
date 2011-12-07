using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadPhotos();
        }

        private Grid RenderPhotoWithCaption(PhotoSummary photo, int index)
        {
            var grid = new Grid();

            var image = new Image();
            var bitmap = new BitmapImage(photo.PhotoUri);
            image.Margin = new Thickness(5, 0, 5, 0);
            image.Source = bitmap;
            image.Height = 650;
            image.Tag = photo.Id;

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
            text.Text = photo.Caption;

            var name = new TextBlock();
            name.Margin = new Thickness(10, 0, 0, 0);
            name.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
            name.Foreground = new SolidColorBrush(Colors.White);
            name.FontSize = 12;
            name.Text = photo.Author;

            var left = new StackPanel();
            left.Orientation = Orientation.Vertical;
            left.Children.Add(text);
            left.Children.Add(name);

            canvas.Children.Add(left);

            grid.Children.Add(image);
            grid.Children.Add(canvas);

            return grid;
        }

        private async void LoadPhotos()
        {
            var photos = await MetroPix.FiveHundredPixels.Site.Query("popular", 50, 4);

            for (int i = 0; i < photos.Count; i++)
            {
                Photos.Children.Add(RenderPhotoWithCaption(photos[i], i));
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
                    Frame.Navigate(typeof(SinglePicture), image.Tag);
                }
            }
        }
    }
}