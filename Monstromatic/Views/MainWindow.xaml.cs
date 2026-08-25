using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Monstromatic.Models;
using ReactiveUI.Avalonia;
using Monstromatic.Utils;
using Monstromatic.ViewModels;
using ReactiveUI;

namespace Monstromatic.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel?.ShowNewMonsterWindow.RegisterHandler(DoShowNewMonster) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowAboutDialog.RegisterHandler(DoShowAboutDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ConfirmResetChanges.RegisterHandler(DoConfirmResetChanges) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowCreateFeatureDialog.RegisterHandler(DoShowCreateFeatureDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowMessage.RegisterHandler(DoShowMessage) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowQualityLevelsDialog.RegisterHandler(DoShowQualityLevelsDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowSkillsEditorDialog.RegisterHandler(DoShowSkillsEditorDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowProfileNameDialog.RegisterHandler(DoShowProfileNameDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.ShowProfileSelectionDialog.RegisterHandler(DoShowProfileSelectionDialog) ?? throw new InvalidOperationException()));
            this.WhenActivated(d => d(ViewModel?.RunProfileSetupWizard.RegisterHandler(DoRunProfileSetupWizard) ?? throw new InvalidOperationException()));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async Task DoConfirmResetChanges(IInteractionContext<Unit, bool> interactionContext)
        {
            var dialog = new ConfirmationWindow("Вы уверены, что хотите сбросить все настройки?");
            var result = await dialog.ShowDialog<bool>(this);
            interactionContext.SetOutput(result);
        }

        private async Task DoShowCreateFeatureDialog(
            IInteractionContext<CreateFeatureViewModel, MonsterFeature?> interactionContext)
        {
            var dialog = new CreateFeatureWindow
            {
                DataContext = interactionContext.Input
            };
            var result = await dialog.ShowDialog<MonsterFeature?>(this);
            interactionContext.SetOutput(result);
        }

        private async Task DoShowMessage(IInteractionContext<MessageRequest, Unit> interactionContext)
        {
            var message = interactionContext.Input;
            var dialog = MessageWindow.Info(message.Title, message.Heading, message.Subtitle, message.Body);
            await dialog.ShowDialog(this);
            interactionContext.SetOutput(Unit.Default);
        }

        private async Task DoShowQualityLevelsDialog(
            IInteractionContext<QualityLevelsViewModel, Dictionary<string, int>?> interactionContext)
        {
            var dialog = new QualityLevelsWindow
            {
                DataContext = interactionContext.Input
            };
            var result = await dialog.ShowDialog<Dictionary<string, int>?>(this);
            interactionContext.SetOutput(result);
        }

        private async Task DoShowSkillsEditorDialog(
            IInteractionContext<SkillsEditorViewModel, SkillsEditorResult?> interactionContext)
        {
            var dialog = new SkillsEditorWindow
            {
                DataContext = interactionContext.Input
            };
            var result = await dialog.ShowDialog<SkillsEditorResult?>(this);
            interactionContext.SetOutput(result);
        }

        private async Task DoShowProfileNameDialog(
            IInteractionContext<ProfileNameViewModel, string?> interactionContext)
        {
            var dialog = new ProfileNameWindow
            {
                DataContext = interactionContext.Input
            };
            var result = await dialog.ShowDialog<string?>(this);
            interactionContext.SetOutput(result);
        }

        private async Task DoShowProfileSelectionDialog(
            IInteractionContext<Unit, ProfileSelectionResult?> interactionContext)
        {
            var dialog = new ProfileSelectionWindow(ViewModel!.ProfileService);
            await dialog.ShowDialog(this);
            interactionContext.SetOutput(dialog.Result);
        }

        private async Task DoRunProfileSetupWizard(IInteractionContext<Unit, bool> interactionContext)
        {
            var isComplete = await ProfileSetupWizard.RunAsync(
                ViewModel!.SettingsProvider,
                isNewProfile: true,
                owner: this);

            interactionContext.SetOutput(isComplete);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private static Task DoShowNewMonster(IInteractionContext<EncounterViewModel, Unit> interactionContext)
        {
            var dialog = new EncounterView
            {
                DataContext = interactionContext.Input
            };
            dialog.Show();
            interactionContext.SetOutput(Unit.Default);
            return Task.CompletedTask;
        }
        
        private async Task DoShowAboutDialog(IInteractionContext<Unit, Unit> interactionContext)
        {
            var dialog = new AboutWindow(ViewModel.ProcessHelper);
            await dialog.ShowDialog(this);
            interactionContext.SetOutput(Unit.Default);
        }
    }
}
