using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
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
        ObservableCollection<PhotoSummary> _boundPhotos;

        public FrontPage()
        {
            InitializeComponent();
            _boundPhotos = new ObservableCollection<PhotoSummary>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Viewer.ItemsSource = _boundPhotos;

            List<PhotoSummary> photos;
            if ((App.Current as App).FirstRun)
            {
                photos = await MetroPix.FiveHundredPixels.Site.Query("popular", 50, 4);
                (App.Current as App).FirstRun = false;
            }
            else
            {
                photos = FiveHundredPixels.Site.LastQuery;
            }
            LoadPhotos(photos);
        }

        async private void LoadPhotos(List<PhotoSummary> photos)
        {
            foreach (var photo in photos)
            {
                var bmp = await photo.GetPhotoAsync();
                photo.Width = bmp.PixelWidth;
                photo.Height = bmp.PixelHeight;

                _boundPhotos.Add(photo);
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //// Home button
            //var photos = await MetroPix.FiveHundredPixels.Site.Query("popular", 50, 4);
            //Photos.Children.Clear();
            //LoadPhotos(photos);
        }

        private void OnPictureClicked(object sender, ItemClickEventArgs e)
        {
            //var image = e.OriginalSource as Image;
            //if (image != null)
            //{
            //    if (image.Tag != null)
            //    {
            //        Frame.Navigate(typeof(LargePhotoPage), image.Tag));
            //    }
            //}
        }
    }
}