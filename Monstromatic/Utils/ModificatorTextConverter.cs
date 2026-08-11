using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Monstromatic.Utils;

/// <summary>
/// Конвертер для отображения модификатора навыка (int).
/// Если значение равно 0 — возвращает пустую строку,
/// иначе форматирует со знаком "+" для положительных значений
/// (отрицательные и так выводятся со знаком "-" стандартно).
/// </summary>
public class ModificatorTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int modificator || modificator == 0)
            return string.Empty;

        return modificator > 0
            ? $"+{modificator}"
            : modificator.ToString(culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack не поддерживается для ModificatorTextConverter.");
    }
}