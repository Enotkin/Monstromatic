using Microsoft.Extensions.DependencyInjection;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.ViewModels;
namespace Monstromatic.Utils;

public class ServiceHub
{
    public static readonly ServiceHub Default = new();

    public readonly ServiceProvider ServiceProvider;

    private ServiceHub()
    {
        var services = new ServiceCollection();
        BuildServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private static void BuildServices(ServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
        services.AddSingleton<TestWindowViewModel>();
        services.AddSingleton<IAppSettingsProvider, AppSettingsProvider>();
        services.AddSingleton<IProcessHelper, ProcessHelper>();
    }
}