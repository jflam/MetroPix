using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class TwelveScreenPictures : Page
    {
        public TwelveScreenPictures()
        {
            this.InitializeComponent();
        }

        private async Task<BitmapImage> NaiveHttpImageDownload(Uri uri)
        {
            using (var client = new HttpClient())
            {
                client.MaxResponseContentBufferSize = Int32.MaxValue;
                var bytes = await client.GetByteArrayAsync(uri);

                using (var ras = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(ras.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                    }

                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(ras);
                    return bitmapImage;
                }
            }
        }

        // TODO: get a list of the 6 photo uris
        //

        private async void LoadBitmapUsingNetworkManager(GridView gridView, Uri photo)
        {
            var image = new Image();
            var bitmap = await NaiveHttpImageDownload(photo);
            image.Height = 300;
            image.Source = bitmap;
            gridView.Items.Add(image);
            //bitmap.ImageOpened += (sender, args) =>
            //{
            //    image.Height = bitmap.PixelHeight;
            //    image.Source = bitmap;
            //    gridView.Items.Add(image);
            //};
        }

        private void LoadBitmapUsingJupiterCodepath(GridView gridView, Uri photo)
        {
            var image = new Image();
            var bitmap = new BitmapImage(photo);
            image.Height = 300;
            image.Source = bitmap;
            gridView.Items.Add(image);
        }

        private string[] _photos = new string[] {
            "http://djlhggipcyllo.cloudfront.net/3644509/1e3fe7550ca6e460a5c9595597a64184c43b25bb/3.jpg",
            "http://djlhggipcyllo.cloudfront.net/3648494/5aeb1f77447d68ae393092c2992ad0caf2178a8b/3.jpg",
            "http://djlhggipcyllo.cloudfront.net/3645240/41309c4dee85bcf98d1225b837f2054008503810/3.jpg",
            "http://djlhggipcyllo.cloudfront.net/3646370/e48038d9666d6aca8a78a2fe45ed1f12450b9b7a/3.jpg",
            "http://djlhggipcyllo.cloudfront.net/3647131/f1d8b6c5eaa0182de70ee2651ce5fea042f6c3d1/3.jpg",
            "http://djlhggipcyllo.cloudfront.net/3629021/c5f379ec9e17e520aad5e0ad8369f266d60b33c0/3.jpg"
        };

        // NOTE: there is a significant difference in working set consumption using the two
        // different code paths that we have here. 
        private void LoadPhotos(GridView gridView, String[] photos)
        {
            foreach (var photo in photos)
            {
                //LoadBitmapUsingJupiterCodepath(gridView, new Uri(photo));
                LoadBitmapUsingNetworkManager(gridView, new Uri(photo));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadPhotos(Photos, _photos);
            LoadPhotos(Photos2, _photos);
        }

        private void GoHome(object sender, RoutedEventArgs args)
        {
            Frame.GoBack();
        }
    }
}