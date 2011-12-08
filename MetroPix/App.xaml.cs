﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
