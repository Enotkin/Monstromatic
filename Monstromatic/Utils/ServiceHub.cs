using Microsoft.Extensions.DependencyInjection;
using Monstromatic.Data;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.Interfaces;
using Monstromatic.Data.Storages;
using Monstromatic.Models;
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
        services.AddSingleton<IAppBaseDataStorage<MonstromaticSettings>, SettingsStorage>();
        services.AddSingleton<IAppSettingsProvider, AppSettingsProvider>();
        services.AddSingleton<IProcessHelper, ProcessHelper>();
    }
}