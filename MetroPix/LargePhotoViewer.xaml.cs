using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroPix
{
    public sealed partial class LargePhotoViewer : UserControl
    {
        public LargePhotoViewer()
        {
            this.InitializeComponent();
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

        public void SetPhotos(List<Uri> photos, int selectedIndex) {
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
            var id = FiveHundredPixels.Site.LastQuery[index].Id;
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

        private bool _menuVisible;

        // TODO: likely replace this with AppBar functionality
        private void PhotoTapped(object sender, TappedRoutedEventArgs e)
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

        private void FireGoBack()
        {
            if (GoBack != null)
            {
                GoBack(this, EventArgs.Empty);
            }
        }

        private void GoBackTapped(object sender, TappedRoutedEventArgs args)
        {
            FireGoBack();
        }

        // TODO: fix this ... need to create a new artist page and render there
        private async void ArtistTapped(object sender, TappedRoutedEventArgs args)
        {
            var id = FiveHundredPixels.Site.LastQuery[Photos.SelectedIndex].Id;
            var photo = await FiveHundredPixels.Site.GetFullSizePhoto(id);
            var photos = await FiveHundredPixels.Site.Query("user:" + photo.UserName, 50);
            FireGoBack();
        }

        public event EventHandler<EventArgs> GoBack;
    }
}