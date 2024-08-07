using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Monstromatic.ViewModels;

namespace Monstromatic.Views
{
    public partial class TestWindowView : ReactiveWindow<TestWindowViewModel>
    {
        public TestWindowView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}