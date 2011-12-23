using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MetroPix
{ 
    sealed partial class App : Application
    {
        private UriDispatcher _parser;

        public App()
        {
            InitializeComponent();
            _parser = new UriDispatcher();
        }

        public PhotoStream PhotoStream { get; set; }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            
            var rootFrame = new Frame();
            PhotoStream = await _parser.Parse(new Uri("http://www.boston.com/bigpicture/2011/12/the_year_in_pictures_part.html"));
            rootFrame.Navigate(typeof(FrontPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            var parser = new UriDispatcher();
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs pargs = (ProtocolActivatedEventArgs)args;
                var uri = new Uri("http://" + pargs.Uri.Host + pargs.Uri.PathAndQuery);
                PhotoStream = await parser.Parse(uri);
                var rootFrame = new Frame();
                rootFrame.Navigate(typeof(FrontPage));
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
        }
    }
}