﻿using System;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MetroPix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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