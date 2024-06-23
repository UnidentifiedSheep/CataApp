using System;
using System.Net.Mime;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ProdajaAltModel : ObservableObject
{
    Regex reg = new(@"[^0-9,.]+");
    [ObservableProperty] private int? _count;

    [ObservableProperty] private int _currencyInitialId;

    [ObservableProperty] private decimal _initialPrice;

    [ObservableProperty] private string? _mainCatName = string.Empty;

    [ObservableProperty] private string? _mainName = string.Empty;

    [ObservableProperty] private int _maxCount;

    [ObservableProperty] private decimal? _price;

    [ObservableProperty] private decimal _priceSum;

    [ObservableProperty] private string _producerName = string.Empty;

    [ObservableProperty] private string? _uniValue = string.Empty;
    [ObservableProperty] private string _textDecimal = "0,00";
    [ObservableProperty] private string _textCont = "0";
    

    public int? Id { get; set; }
    public int ProdajaId { get; set; }
    public int? MainCatId { get; set; }

    partial void OnTextContChanged(string value)
    {
        value = reg.Replace(value, "").Replace(",","").Replace(".","").TrimStart('0');
        if (string.IsNullOrEmpty(value))
        {
            Count = 0;
            TextCont = "0";
        }
        else
        {
            if (Convert.ToInt32(value) > MaxCount)
            {
                Count = MaxCount;
                TextCont = Convert.ToString(MaxCount);
            }
            else
            {
                Count = Convert.ToInt32(value);
                TextCont = value;
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