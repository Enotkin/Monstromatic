namespace Monstromatic.Models;

/// <summary>
/// Выбор пользователя в окне профилей.
/// </summary>
/// <param name="Remember">
/// Новое состояние галочки «Запомнить выбор». Пусто — настройку не трогаем:
/// так происходит при создании профиля, чтобы прежний запомненный выбор не
/// сбрасывался незаметно для пользователя.
/// </param>
/// <param name="IsNew">
/// Профиль только что создан и его нужно провести через мастер настройки.
/// </param>
public record ProfileSelectionResult(Profile Profile, bool? Remember, bool IsNew);
