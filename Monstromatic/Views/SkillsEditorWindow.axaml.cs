using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Monstromatic.Models;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class SkillsEditorWindow : Window
{
    public SkillsEditorWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Итог работы окна. Дублирует результат ShowDialog, чтобы окно можно было
    /// использовать и как шаг мастера, когда модального диалога ещё нет.
    /// </summary>
    public SkillsEditorResult? Result { get; private set; }

    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SkillsEditorViewModel viewModel || !viewModel.CanSave)
        {
            return;
        }

        // При первичной настройке профиля ограничений нет: набор ещё только
        // складывается. Особенности, оставшиеся без скилла, программа тихо
        // приберёт сама — иначе она упадёт на первом же монстре.
        if (!viewModel.IsWizardStep && !await ConfirmFeatureChangesAsync(viewModel))
        {
            return;
        }

        Result = viewModel.CreateResult();
        Close(Result);
    }

    /// <summary>
    /// Согласовывает с пользователем судьбу особенностей, завязанных на
    /// удаляемые скиллы. false — пользователь передумал, окно остаётся открытым
    /// и правки не теряются.
    /// </summary>
    private async Task<bool> ConfirmFeatureChangesAsync(SkillsEditorViewModel viewModel)
    {
        // Особенность, которую уже использует монстр из бестиария, удалять
        // нельзя — значит нельзя удалить и скилл, на котором она держится.
        var blocked = viewModel.GetBlockedFeaturesMessage();
        if (blocked is not null)
        {
            var message = MessageWindow.Info(
                "Удаление скиллов",
                "Эти скиллы пока нельзя удалить",
                "От них зависят особенности, которые уже используют монстры из бестиария.",
                blocked);

            await message.ShowDialog(this);
            return false;
        }

        // Остальные зависимые особенности можно удалить, но только с согласия.
        var warning = viewModel.GetFeatureRemovalWarning();
        if (warning is null)
        {
            return true;
        }

        var confirmation = MessageWindow.Confirmation(
            "Удаление скиллов",
            "Эти особенности перестанут работать",
            "Они настроены на скиллы, которых больше не будет в наборе.",
            warning,
            "Удалить особенности");

        return await confirmation.ShowDialog<bool>(this);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(default(SkillsEditorResult));
    }
}
