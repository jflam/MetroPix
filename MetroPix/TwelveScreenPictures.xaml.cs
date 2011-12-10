using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class TwelveScreenPictures : Page
    {
        public TwelveScreenPictures()
        {
            this.InitializeComponent();
        }

        private async void LoadPhotos(GridView gridView, List<PhotoSummary> photos)
        {
            foreach (var photo in photos)
            {
                var image = new Image();
                var bitmap = await photo.GetPhotoAsync();
                bitmap.ImageOpened += (sender, args) =>
                {
                    image.Height = bitmap.PixelHeight;
                    image.Source = bitmap;
                    gridView.Items.Add(image);
                };
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var photos = await FiveHundredPixels.Site.Query("popular", 6, 3);
            LoadPhotos(Photos, photos);
            LoadPhotos(Photos2, photos);
        }
    }
}