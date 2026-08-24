using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Monstromatic.Models;
using ReactiveUI.Avalonia;
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
            this.WhenActivated(d => d(ViewModel?.ShowFeatureDeletionWarning.RegisterHandler(DoShowFeatureDeletionWarning) ?? throw new InvalidOperationException()));
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

        private async Task DoShowFeatureDeletionWarning(IInteractionContext<string, Unit> interactionContext)
        {
            var dialog = new FeatureDeletionWarningWindow(interactionContext.Input);
            await dialog.ShowDialog(this);
            interactionContext.SetOutput(Unit.Default);
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
