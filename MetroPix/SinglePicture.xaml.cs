using System;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    public sealed partial class SinglePicture : Page
    {
        private bool _menuVisible;
        private GestureRecognizer _recognizer = new GestureRecognizer();

        public SinglePicture()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int id = Convert.ToInt32(e.Parameter);
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            Photo.Source = new BitmapImage(photo.PhotoUri);
            Caption.Text = photo.Caption;
            Artist.Text = photo.Artist;
            Rating.Text = String.Format("{0:F1}", photo.Rating);
            Views.Text = photo.Views.ToString();
            Votes.Text = photo.Votes.ToString();
            Favs.Text = photo.Votes.ToString();
        }

        private void Photo_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void Photo_ManipulationCompleted_1(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

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
        }

        private void Photo_ManipulationStarting_1(object sender, ManipulationStartingRoutedEventArgs e)
        {
        }

        private void Photo_ManipulationDelta_1(object sender, ManipulationDeltaRoutedEventArgs e)
        {
        }
    }
}
