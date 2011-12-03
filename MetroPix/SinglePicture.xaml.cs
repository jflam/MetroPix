using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class SinglePicture : Page
    {
        public SinglePicture()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int id = Convert.ToInt32(e.Parameter);
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            Photo.Source = new BitmapImage(new Uri(photo["photo"].GetObject()["image_url"].GetString()));
            Title.Text = photo["photo"].GetObject()["name"].GetString();
            Artist.Text = photo["photo"].GetObject()["user"].GetObject()["fullname"].GetString();
        }

        private void Photo_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
