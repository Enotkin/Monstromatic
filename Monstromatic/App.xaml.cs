using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Monstromatic.Utils;

namespace Monstromatic
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var startup = new ApplicationStartup(desktop, ServiceHub.Default.ServiceProvider);
                _ = RunStartupAsync(startup, desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static async System.Threading.Tasks.Task RunStartupAsync(
            ApplicationStartup startup,
            IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                await startup.RunAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                desktop.Shutdown(1);
            }
        }
    }
}
