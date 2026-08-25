using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Monstromatic.Views;

/// <summary>
/// Универсальное окно сообщения: либо просто уведомление с кнопкой «Понятно»,
/// либо вопрос с двумя кнопками. Результат возвращается как <see cref="bool"/>:
/// true — пользователь подтвердил действие.
/// </summary>
public partial class MessageWindow : Window
{
    public MessageWindow()
    {
        InitializeComponent();
    }

    private MessageWindow(
        string title,
        string heading,
        string subtitle,
        string body,
        string? confirmText,
        string cancelText)
        : this()
    {
        Title = title;
        HeadingTextBlock.Text = heading;
        SubtitleTextBlock.Text = subtitle;
        SubtitleTextBlock.IsVisible = !string.IsNullOrWhiteSpace(subtitle);
        BodyTextBlock.Text = body;
        BodyTextBlock.IsVisible = !string.IsNullOrWhiteSpace(body);
        ConfirmButton.Content = confirmText;
        ConfirmButton.IsVisible = confirmText is not null;
        CancelButton.Content = cancelText;
    }

    public static MessageWindow Info(string title, string heading, string subtitle, string body) =>
        new(title, heading, subtitle, body, null, "Понятно");

    public static MessageWindow Confirmation(
        string title,
        string heading,
        string subtitle,
        string body,
        string confirmText,
        string cancelText = "Отмена") =>
        new(title, heading, subtitle, body, confirmText, cancelText);

    private void ConfirmButton_OnClick(object? sender, RoutedEventArgs e) => Close(true);

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}
