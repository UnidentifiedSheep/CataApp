using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace CatalogueAvalonia.Core.ConverterV;

internal class NullBlocker : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(System.Convert.ToString(value)))
        {
            return 0m;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(System.Convert.ToString(value)))
        {
            return 0m;
        }

        return value;
    }
}