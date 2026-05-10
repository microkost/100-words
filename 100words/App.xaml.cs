using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace words100
{
    sealed partial class App : Application
    {
        private Window _window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Window();

            Frame rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage), args.Arguments);

            _window.Content = rootFrame;
            _window.Title = "100 Words";
            _window.Activate();
        }
    }
}

