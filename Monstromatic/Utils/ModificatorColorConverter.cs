using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Monstromatic.Utils;

/// <summary>
/// Конвертер для цвета значения навыка (int Modificator).
/// Если значение отклонилось от базового (Modificator != 0) — фиолетовый,
/// иначе — обычный белый цвет.
/// </summary>
public class ModificatorColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int modificator && modificator != 0)
            return Brushes.MediumPurple;

        return Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack не поддерживается для ModificatorColorConverter.");
    }
}
