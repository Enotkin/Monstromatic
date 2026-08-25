using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Monstromatic.Data.Profiles;
using Monstromatic.Models;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class ProfileSelectionWindow : Window
{
    private readonly IProfileService? _profileService;

    public ProfileSelectionWindow()
    {
        InitializeComponent();
    }

    public ProfileSelectionWindow(IProfileService profileService)
        : this()
    {
        _profileService = profileService;
        DataContext = new ProfileSelectionViewModel(
            profileService.Profiles,
            profileService.RememberedProfileId);
    }

    public ProfileSelectionResult? Result { get; private set; }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_profileService is null || DataContext is not ProfileSelectionViewModel viewModel)
        {
            return;
        }

        var nameWindow = new ProfileNameWindow
        {
            DataContext = new ProfileNameViewModel(
                "Название нового профиля",
                string.Empty,
                viewModel.Profiles.Select(profile => profile.Name))
        };

        var name = await nameWindow.ShowDialog<string?>(this);
        if (name is null)
        {
            return;
        }

        var profile = _profileService.Create(name);
        viewModel.Add(profile);

        // Новый профиль сразу отправляем в мастер настройки. Галочку
        // «Запомнить выбор» при этом не применяем: она относилась к тому
        // профилю, который был выбран в списке до нажатия кнопки.
        Result = new ProfileSelectionResult(profile, Remember: null, IsNew: true);
        Close();
    }

    private void OpenButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileSelectionViewModel { SelectedProfile: { } profile } viewModel)
        {
            return;
        }

        Result = new ProfileSelectionResult(profile, viewModel.RememberChoice, IsNew: false);
        Close();
    }
}
