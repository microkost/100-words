using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System;
using System.Runtime.InteropServices;
using WinRT;

namespace words100
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Bootstrap.Initialize(0x00010006); // Windows App SDK 1.6

            ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });

            Bootstrap.Shutdown();
        }
    }
}
