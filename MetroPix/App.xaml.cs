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
            await FiveHundredPixels.Site.Query("popular", 20);
            rootFrame.Navigate(typeof(SinglePhotoPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }
    }
}
