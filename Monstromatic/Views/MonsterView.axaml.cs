using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Monstromatic.Views
{
    public partial class MonsterView : UserControl
    {
        private bool _isExpanded = true;
        
        public MonsterView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void ChangeExpandState(object sender, RoutedEventArgs routedEventArgs)
        {
            _isExpanded = !_isExpanded;
            var grid = this.GetControl<Grid>("ExpanderGrid");
            grid.Height = _isExpanded ? 200 : 0;
     
        }
    }
}