using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MetroPix
{ 
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = new Frame();
            await FiveHundredPixels.Site.Query("editors", 50);
            rootFrame.Navigate(typeof(LargePhotoPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }
    }
}
