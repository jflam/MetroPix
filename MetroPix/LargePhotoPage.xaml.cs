﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class LargePhotoPage : Page
    {
        public LargePhotoPage()
        {
            this.InitializeComponent();
            Photos.GoBack += Photos_GoBack;
        }

        void Photos_GoBack(object sender, EventArgs e)
        {
            Frame.GoBack();
        }

        private void InitializePhotos(int selectedIndex)
        {
            // TODO: update user control to consume a List<PhotoSummary> instead
            var photos = new List<Uri>();
            foreach (var photo in FiveHundredPixels.Site.LastQuery)
            {
                photos.Add(photo.PhotoUri);
            }
            Photos.SetPhotos(photos, selectedIndex);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var selectedIndex = Convert.ToInt32(e.Parameter);
            InitializePhotos(selectedIndex);
        }
    }
}
