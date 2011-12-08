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

        private List<PhotoSummary> _photos;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _photos = await FiveHundredPixels.Site.Query("popular", 50, 4);
            Photos.ItemsSource = _photos;
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
                TopMenuEntryAnimation.Begin();
                BottomMenuEntryAnimation.Begin();
            }
            _menuVisible = !_menuVisible;
            e.Handled = true;
        }

        private async void Photos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Photos.SelectedIndex >= 0)
            {
                var id = _photos[Photos.SelectedIndex].Id;
                var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
                Caption.Text = photo.Caption;
                Artist.Text = photo.Artist;
                Rating.Text = String.Format("{0:F1}", photo.Rating);
                Views.Text = photo.Views.ToString();
                Votes.Text = photo.Votes.ToString();
                Favs.Text = photo.Votes.ToString();
            }
        }
    }
}