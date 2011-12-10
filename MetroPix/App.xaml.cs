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

        public bool FirstRun = true;

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(FrontPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }
    }
}
