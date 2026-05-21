using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;

namespace words100
{
    sealed partial class App : Application
    {
        private Window? _window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Window();

            if (MicaController.IsSupported())
                _window.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };

            Frame rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage), args.Arguments);

            _window.Content = rootFrame;
            _window.Title = "100 Words";

            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "appicon.ico");
            if (File.Exists(iconPath))
                _window.AppWindow.SetIcon(iconPath);

            _window.Activate();
        }
    }
}

