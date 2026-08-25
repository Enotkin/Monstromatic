using System.Threading.Tasks;
using Avalonia.Controls;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.ViewModels;
using Monstromatic.Views;

namespace Monstromatic.Utils;

/// <summary>
/// Проводит пользователя по обязательным настройкам профиля: сначала стартовые
/// уровни, затем скиллы. Без них программой пользоваться нельзя, поэтому отказ
/// на любом шаге прерывает весь мастер.
/// </summary>
public static class ProfileSetupWizard
{
    /// <param name="isNewProfile">
    /// У нового профиля шаги показываются всегда, у существующего — только если
    /// нужные данные пропали.
    /// </param>
    /// <param name="owner">
    /// Окно-владелец. На старте программы его ещё нет, тогда шаги показываются
    /// обычными окнами.
    /// </param>
    public static async Task<bool> RunAsync(
        IAppSettingsProvider settingsProvider,
        bool isNewProfile,
        Window? owner = null)
    {
        if (isNewProfile || settingsProvider.Settings.MonsterQualities.Count == 0)
        {
            var qualitiesWindow = new QualityLevelsWindow
            {
                DataContext = new QualityLevelsViewModel(
                    settingsProvider.Settings.MonsterQualities,
                    isWizardStep: true)
            };

            await ShowAsync(qualitiesWindow, owner);
            if (qualitiesWindow.Result is null)
            {
                return false;
            }

            settingsProvider.ApplyQualities(qualitiesWindow.Result);
        }

        if (isNewProfile || settingsProvider.Settings.SkillDefinitions.Count == 0)
        {
            var skillsWindow = new SkillsEditorWindow
            {
                DataContext = new SkillsEditorViewModel(
                    settingsProvider.Settings.SkillDefinitions,
                    settingsProvider.Settings.MonsterQualities,
                    settingsProvider.Features,
                    isWizardStep: true)
            };

            await ShowAsync(skillsWindow, owner);
            if (skillsWindow.Result is null)
            {
                return false;
            }

            settingsProvider.ApplySkills(skillsWindow.Result);
        }

        return true;
    }

    private static Task ShowAsync(Window window, Window? owner) =>
        owner is null
            ? window.ShowStandaloneAsync()
            : window.ShowDialog(owner);
}
