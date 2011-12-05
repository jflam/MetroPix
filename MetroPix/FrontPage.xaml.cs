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
        // Note that the .ctor fires here even if we are returning from a different page -- where do we store state?
        public FrontPage()
        {
            InitializeComponent();
        }

        // TODO: figure out how to do the right kind of data binding here?! What kind of control do I need? A ListView?
        private async void LoadPhotos()
        {
            var photos = await FiveHundredPixels.Site.Query("popular", 20);
            foreach (var photo in photos)
            {
                var photoUri = FiveHundredPixels.Site.GetPhotoUri(photo, 4);
                var grid = new Grid();

                var image = new Image();
                image.Margin = new Thickness(5, 0, 5, 0);
                image.Source = new BitmapImage(photoUri);
                image.Height = 650;
                image.Tapped += (sender, args) =>
                {
                    // BUGBUG: all event handlers are not wired up when I go back!
                    Frame.Navigate(typeof(SinglePicture).FullName, photo.Id);
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

                Photos.Children.Add(grid);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadPhotos();
        }
    }
}
