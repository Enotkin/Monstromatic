namespace Monstromatic.ViewModels;

/// <summary>
/// Текст для универсального окна-сообщения.
/// </summary>
public record MessageRequest(string Title, string Heading, string Subtitle, string Body);
