using System;
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
            await HtmlImporter.Site.Query(new Uri("http://www.boston.com/bigpicture/2011/12/the_year_in_pictures_part.html"));
            //await RssImporter.Site.Query(new Uri("http://feeds.boston.com/boston/bigpicture/index"));
            //await ImgurImporter.Site.Query(new Uri("http://imgur.com/r/aww/top"));
            //await RedditImporter.Site.Query(new Uri("http://reddit.com/r/aww"));
            //await FiveHundredPixels.Site.Query("popular", 20);
            rootFrame.Navigate(typeof(FrontPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs pargs = (ProtocolActivatedEventArgs)args;
                var uri = new Uri("http://" + pargs.Uri.Host + pargs.Uri.PathAndQuery);
                await HtmlImporter.Site.Query(uri);
                var rootFrame = new Frame();
                rootFrame.Navigate(typeof(Home));
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
        }
    }
}
