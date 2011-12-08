using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class FullScreenPictures : Page
    {
        public FullScreenPictures()
        {
            this.InitializeComponent();
        }

        private async void CreateFlipViewItems(FlipView flipView)
        {
            foreach (var photo in FiveHundredPixels.Site.LastQuery)
            {
                // Awaiting an async get is exactly what is needed here
                // However, there is an animation bug in FlipView where
                // the animation terminates 
                var image = new Image();
                image.Source = await photo.GetPhotoAsync();
                flipView.Items.Add(image);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CreateFlipViewItems(Photos);
            Photos.SelectedIndex = Convert.ToInt32(e.Parameter);
        }

        private void Photo_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private bool _menuVisible;

        private void Photo_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            if (_menuVisible)
            {
                TopMenuExitAnimation.Begin();
                BottomMenuExitAnimation.Begin();
            }
            else
            {
                UpdatePhotoCaption(Photos.SelectedIndex);
                TopMenuEntryAnimation.Begin();
                BottomMenuEntryAnimation.Begin();
            }
            _menuVisible = !_menuVisible;
            e.Handled = true;
        }

        private async void UpdatePhotoCaption(int index)
        {
            var id = FiveHundredPixels.Site.LastQuery[index].Id;
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            Caption.Text = photo.Caption;
            Artist.Text = photo.Artist;
            Rating.Text = String.Format("{0:F1}", photo.Rating);
            Views.Text = photo.Views.ToString();
            Votes.Text = photo.Votes.ToString();
            Favs.Text = photo.Votes.ToString();
        }

        private void Photos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Photos.SelectedIndex >= 0)
            {
                UpdatePhotoCaption(Photos.SelectedIndex);
            }
        }

        // TODO: very inefficient -- need to design a proper data model
        private async void Artist_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            var id = FiveHundredPixels.Site.LastQuery[Photos.SelectedIndex].Id;
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            var photos = await FiveHundredPixels.Site.Query("user:" + photo.UserName, 50);
            Frame.GoBack();
        }
    }
}