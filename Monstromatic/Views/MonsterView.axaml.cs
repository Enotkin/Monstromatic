using Avalonia;
using Avalonia.Controls;
using System.Linq;
using Avalonia.VisualTree;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Monstromatic.ViewModels;

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
            var grid = this.GetControl<ItemsControl>("ExpanderGrid");
            if (!_isExpanded)
                grid.GetVisualDescendants().OfType<SkillLensPanel>().FirstOrDefault()?.ResetSelection();
            grid.Height = _isExpanded ? double.NaN : 0;
            // Clipping is disabled for the floating lens, so collapse must also
            // hide the content instead of relying on a zero-height clip.
            grid.IsVisible = _isExpanded;
            
            AnimateButton(sender as Visual, _isExpanded);
        }
        
        private void AnimateButton(Visual button, in bool isExpanded)
        {
            if (button?.RenderTransform is RotateTransform transform) 
                transform.Angle = isExpanded? 0 : 180;
        }

        private void DeathToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as Avalonia.Controls.Primitives.ToggleButton;
            SetDeathOverlayVisibility(toggleButton?.IsChecked == true);
        }

        private void SetDeathOverlayVisibility(bool isVisible)
        {
            var overlay = this.GetControl<Border>("DeathOverlay");
            overlay.IsVisible = isVisible;
        }

        private void MonsterName_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var nameEditor = this.GetControl<TextBox>("MonsterNameEditor");
            if (e.ClickCount != 2 || nameEditor.IsVisible || DataContext is not MonsterViewModel viewModel)
            {
                return;
            }

            viewModel.BeginNameEditing();

            var nameText = this.GetControl<TextBlock>("MonsterNameText");
            nameText.IsVisible = false;
            nameEditor.IsVisible = true;

            e.Handled = true;

            Dispatcher.UIThread.Post(() =>
            {
                nameEditor.Focus();
                nameEditor.SelectAll();
            }, DispatcherPriority.Input);
        }

        private void MonsterNameEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitNameEditing();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelNameEditing();
                e.Handled = true;
            }
        }

        private void MonsterNameEditor_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.GetControl<TextBox>("MonsterNameEditor").IsVisible)
            {
                CommitNameEditing();
            }
        }

        private void CommitNameEditing()
        {
            if (DataContext is MonsterViewModel viewModel)
            {
                viewModel.CommitNameEditing();
            }

            FinishNameEditing();
        }

        private void CancelNameEditing()
        {
            if (DataContext is MonsterViewModel viewModel)
            {
                viewModel.CancelNameEditing();
            }

            FinishNameEditing();
        }

        private void FinishNameEditing()
        {
            this.GetControl<TextBox>("MonsterNameEditor").IsVisible = false;
            this.GetControl<TextBlock>("MonsterNameText").IsVisible = true;
        }
    }
}
