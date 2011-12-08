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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Photos.ItemsSource = FiveHundredPixels.Site.LastQuery;
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
                var id = FiveHundredPixels.Site.LastQuery[Photos.SelectedIndex].Id;
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