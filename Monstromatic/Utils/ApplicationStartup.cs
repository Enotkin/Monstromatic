using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.Bestiary;
using Monstromatic.Data.Profiles;
using Monstromatic.Models;
using Monstromatic.ViewModels;
using Monstromatic.Views;

namespace Monstromatic.Utils;

/// <summary>
/// Сценарий запуска программы: выбрать профиль, при необходимости провести
/// пользователя через настройку стартовых уровней и скиллов и только потом
/// открыть главное окно.
/// </summary>
public class ApplicationStartup
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly ServiceProvider _services;

    public ApplicationStartup(IClassicDesktopStyleApplicationLifetime desktop, ServiceProvider services)
    {
        _desktop = desktop;
        _services = services;
    }

    public async Task RunAsync()
    {
        // До появления главного окна закрывать программу может только сам сценарий.
        _desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var profileService = _services.Get<IProfileService>();
        profileService.Initialize();

        var selection = await ChooseProfileAsync(profileService);
        if (selection is null)
        {
            _desktop.Shutdown();
            return;
        }

        profileService.SetCurrent(selection.Profile);

        var settingsProvider = _services.Get<IAppSettingsProvider>();
        var bestiaryService = _services.Get<IBestiaryService>();
        var profileDirectory = profileService.CurrentProfileDirectory;
        settingsProvider.UseProfile(profileDirectory);
        bestiaryService.UseProfile(profileDirectory);

        if (!await ProfileSetupWizard.RunAsync(settingsProvider, selection.IsNew))
        {
            // Мастер отменили — незаполненный профиль оставлять нельзя.
            if (selection.IsNew)
            {
                profileService.Delete(selection.Profile);
            }

            _desktop.Shutdown();
            return;
        }

        ShowMainWindow();
    }

    private static async Task<ProfileSelectionResult?> ChooseProfileAsync(IProfileService profileService)
    {
        // Единственный профиль — про выбор пользователю знать незачем.
        if (profileService.Profiles.Count == 1)
        {
            return new ProfileSelectionResult(profileService.Profiles[0], Remember: null, IsNew: false);
        }

        var remembered = profileService.Profiles
            .FirstOrDefault(profile => profile.Id == profileService.RememberedProfileId);

        if (remembered is not null)
        {
            return new ProfileSelectionResult(remembered, Remember: null, IsNew: false);
        }

        var window = new ProfileSelectionWindow(profileService);
        await window.ShowStandaloneAsync();

        var result = window.Result;
        if (result?.Remember is { } remember)
        {
            profileService.Remember(remember ? result.Profile : null);
        }

        return result;
    }

    private void ShowMainWindow()
    {
        var mainWindow = new MainWindow
        {
            DataContext = _services.Get<MainWindowViewModel>()
        };

        _desktop.MainWindow = mainWindow;
        _desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }
}
