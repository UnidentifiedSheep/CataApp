using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CatalogueAvalonia.Core.ConverterV;

public class FromStatusToActionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (int?)value;
        if (status != null)
        {
            if (status == 0)
                return "Выплата нам";
            if (status == 1)
                return "Выплата контрагенту";
            if (status == 2)
                return "Закупка";
            if (status == 4)
                return "Продажа";
            return value;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}