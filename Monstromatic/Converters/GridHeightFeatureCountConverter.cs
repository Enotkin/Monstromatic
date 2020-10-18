﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Monstromatic.Models;

namespace Monstromatic.Converters
{
    public class GridHeightFeatureCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = GetParameters(parameter.ToString());
            var enumerable = value as IEnumerable<FeatureBase>;
            return (parameters.Item1 + enumerable.Count() * parameters.Item2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private (int, int) GetParameters(string parameterString)
        {
            var parameters = parameterString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return (int.Parse(parameters[0]), int.Parse(parameters[1]));
        }
    }
}
