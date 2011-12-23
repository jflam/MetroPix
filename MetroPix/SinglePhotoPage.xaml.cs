using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MetroPix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SinglePhotoPage : Page
    {
        public SinglePhotoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var selectedIndex = Convert.ToInt32(e.Parameter);
            var photos = new List<Uri>();
            foreach (var photo in (App.Current as App).Photos)
            {
                photos.Add(photo.PhotoUri);
            }
            SetPhotos(photos, selectedIndex);
        }

        private int _cacheSize = 3;
        private List<Uri> _photos;
        private int _currentIndex = -1;

        private void CreatePhotoPlaceHolders()
        {
            for (int i = 0; i < _photos.Count; i++)
            {
                Photos.Items.Add(new Image());
            }
        }

        private async void LoadImages(int index, int count)
        {
            if (index == -1)
                return;

            if (index + count > _photos.Count)
            {
                count = _photos.Count - index;
            }

            for (int i = index; i < index + count; i++)
            {
                var uri = _photos[i];
                var bitmapImage = await NetworkManager.Current.GetBitmapImageAsync(uri);
                var image = new Image();
                image.Source = bitmapImage;
                Photos.Items[i] = image;
            }
        }

        private void FreeImages(int index)
        {
            if (index >= 0)
            {
                // Replace existing item with a new Image ... GC will eventually clean up the original Image? 
                // It appears that I have a memory leak here though...
                //(Photos.Items[index] as Image).Source = null;
                Photos.Items[index] = new Image();
            }
        }

        private void MoveForward(int currentIndex)
        {
            int imageIndexToLoad = Math.Min(currentIndex + _cacheSize, _photos.Count - 1);
            int imageIndexToFree = Math.Max(-1, currentIndex - _cacheSize - 1);
            LoadImages(imageIndexToLoad, 1);
            FreeImages(imageIndexToFree);
        }

        private void MoveBackward(int currentIndex)
        {
            int imageIndexToLoad = Math.Max(-1, currentIndex - _cacheSize);
            int imageIndexToFree = Math.Min(currentIndex + _cacheSize + 1, _photos.Count - 1);
            LoadImages(imageIndexToLoad, 1);
            FreeImages(imageIndexToFree);
        }

        public void SetPhotos(List<Uri> photos, int selectedIndex)
        {
            _photos = photos;
            CreatePhotoPlaceHolders();
            var startIndex = Math.Max(0, selectedIndex - _cacheSize);
            var count = _cacheSize * 2 + 1;
            LoadImages(startIndex, count);
            Photos.SelectionChanged += CachingFlipView_SelectionChanged;
            Photos.SelectedIndex = selectedIndex;
        }

        private async void UpdatePhotoCaption(int index)
        {
            var photos = (App.Current as App).Photos;
            var id = photos[index].Id;
            // TODO: better data model for photos
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            Caption.Text = photo.Caption;
            Artist.Text = photo.Artist;
            Rating.Text = String.Format("{0:F1}", photo.Rating);
            Views.Text = photo.Views.ToString();
            Votes.Text = photo.Votes.ToString();
            Favs.Text = photo.Votes.ToString();
        }

        void CachingFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int newIndex = Photos.SelectedIndex;
            if (newIndex == -1 || newIndex == _currentIndex)
            {
                return;
            }
            else if (newIndex > _currentIndex)
            {
                MoveForward(newIndex);
            }
            else if (newIndex < _currentIndex)
            {
                MoveBackward(newIndex);
            }
            UpdatePhotoCaption(newIndex);
            _currentIndex = newIndex;
        }

        private void GoBackTapped(object sender, RoutedEventArgs args)
        {
            Frame.GoBack();
        }

        private async void ArtistTapped(object sender, RoutedEventArgs args)
        {
            var id = FiveHundredPixels.Site.LastQuery[Photos.SelectedIndex].Id;
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            var photos = await FiveHundredPixels.Site.Query("user:" + photo.UserName, 50);
            Frame.GoBack();
        }
    }
}
