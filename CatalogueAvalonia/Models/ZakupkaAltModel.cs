using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ZakupkaAltModel : ObservableObject
{
    Regex reg = new(@"[^0-9,.]+");
    [ObservableProperty] private bool _canDelete = true;

    [ObservableProperty] int? _count;

    [ObservableProperty] string? _mainCatName = string.Empty;

    [ObservableProperty]  string? _mainName = string.Empty;

    [ObservableProperty] private int _minCount;

    [ObservableProperty] decimal? _price;

    [ObservableProperty] private decimal _priceSum;

    [ObservableProperty] string? _uniValue = string.Empty;
    [ObservableProperty] private string _textDecimal = "0,00";
    [ObservableProperty] private string _textCount = "0";

    partial void OnTextCountChanged(string value)
    {
        value = reg.Replace(value, "").Replace(",","").Replace(".","").TrimStart('0');
        if (string.IsNullOrEmpty(value))
        {
            Count = MinCount;
            TextCount = Convert.ToString(MinCount);
        }
        else
        {
            if (Convert.ToInt32(value) < MinCount)
            {
                Count = MinCount;
                TextCount = Convert.ToString(MinCount);
            }
            else
            {
                Count = Convert.ToInt32(value);
                TextCount = value;
            }
        }
    }

    partial void OnTextDecimalChanged(string value)
    {
        value = reg.Replace(value, "").TrimStart('0');
        if (string.IsNullOrEmpty(value))
        {
            TextDecimal = "0,00";
            Price = 0m;
        }
        else
        {
            
            var endPrice = Math.Round(Convert.ToDecimal(value.Replace('.', ',')), 2);
            TextDecimal = Convert.ToString(endPrice);
            Price = endPrice;
        }
    }

    public int? Id { get; set; }
    public int ZakupkaId { get; set; }
    public int? MainCatId { get; set; }
    public ProducerModel? ProducerModel;

    partial void OnPriceChanged(decimal? value)
    {
        Price = Math.Round(value ?? 0, 2);
        PriceSum = Math.Round((Price ?? 0) * (Count ?? 0), 2);
    }

    partial void OnCountChanged(int? value)
    {
        PriceSum = Math.Round((Price ?? 0) * (Count ?? 0), 2);
    }
    
}