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
#if !PACKAGED
            // Unpackaged (local dev): manually bootstrap the Windows App SDK runtime.
            // For Store/MSIX builds, the runtime is part of the package - skip this.
            Bootstrap.Initialize(0x00020000); // Windows App SDK 2.0
#endif

            ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });

#if !PACKAGED
            Bootstrap.Shutdown();
#endif
        }
    }
}
