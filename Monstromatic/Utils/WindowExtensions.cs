using System.Threading.Tasks;
using Avalonia.Controls;

namespace Monstromatic.Utils;

public static class WindowExtensions
{
    /// <summary>
    /// Показывает окно и ждёт, пока его закроют. Нужно на старте программы,
    /// когда главного окна ещё нет и показать модальный диалог не над чем.
    /// Результат окна читается из его свойства Result.
    /// </summary>
    public static Task ShowStandaloneAsync(this Window window)
    {
        var completion = new TaskCompletionSource();
        window.Closed += (_, _) => completion.TrySetResult();
        window.Show();
        return completion.Task;
    }
}
