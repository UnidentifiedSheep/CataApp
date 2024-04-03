using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CatalogueAvalonia.Core.ConverterV;

public class FromStatusToActionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int? status = (int?)value;
        if (status != null)
        {
            if (status == 0)
                return "Выплата нам";
            else if (status == 1)
                return "Выплата контрагенту";
            else if (status == 2)
                return "Закупка";
            else if (status == 4)
                return "Продажа";
            else
            {
                return value;
            }
            
        }
        else
            return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}