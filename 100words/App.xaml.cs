using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
            _window.Activate();
        }
    }
}

