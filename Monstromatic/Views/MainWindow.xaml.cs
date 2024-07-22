using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Monstromatic.ViewModels;
using ReactiveUI;

namespace Monstromatic.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel?.ShowNewMonsterWindow.RegisterHandler(DoShowNewMonster)));
            this.WhenActivated(d => d(ViewModel?.ShowAboutDialog.RegisterHandler(DoShowAboutDialog)));
            this.WhenActivated(d => d(ViewModel?.ConfirmResetChanges.RegisterHandler(DoConfirmResetChanges)));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async Task DoConfirmResetChanges(InteractionContext<Unit, bool> interaction)
        {
            var dialog = new ConfirmationWindow("Вы уверены, что хотите сбросить все настройки?");
            var result = await dialog.ShowDialog<bool>(this);
            interaction.SetOutput(result);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private static void DoShowNewMonster(InteractionContext<EncounterViewModel, Unit> interaction)
        {
            var dialog = new EncounterView
            {
                DataContext = interaction.Input
            };
            dialog.Show();
            interaction.SetOutput(Unit.Default);
        }
        
        private async Task DoShowAboutDialog(InteractionContext<Unit, Unit> interaction)
        {
            var dialog = new AboutWindow(ViewModel.ProcessHelper);
            await dialog.ShowDialog(this);
            interaction.SetOutput(Unit.Default);
        }
    }
}
