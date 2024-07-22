using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;

namespace Monstromatic.Views
{
    public partial class MonsterView : UserControl
    {
        public MonsterView()
        {
            AvaloniaXamlLoader.Load(this);
            var header = this.GetControl<CustomHeader>("Header");
            header.ExpandedStateSender += ChangeExpandState;
        }

        public static readonly StyledProperty<IBrush> BorderColorProperty =
            AvaloniaProperty.Register<MonsterView, IBrush>(nameof(CustomBorderBrush));
        
        public IBrush CustomBorderBrush
        {
            get => GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }
        
        private void ChangeExpandState(bool state)
        {
            var grid = this.GetControl<Grid>("ExpanderGrid");
            grid.Height = state ? 200 : 0;
        }

        public void ChangeColor(IBrush brush)
        {
            var border = this.GetControl<Border>("Border");
            border.BorderBrush = brush;
        }
    }
}