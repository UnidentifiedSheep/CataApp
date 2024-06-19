using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ZakupkaAltModel : ObservableObject
{
    [ObservableProperty] private bool _canDelete = true;

    [ObservableProperty] int? _count;

    [ObservableProperty] string? _mainCatName = string.Empty;

    [ObservableProperty]  string? _mainName = string.Empty;

    [ObservableProperty] private int _minCount;

    [ObservableProperty]  decimal? _price;

    [ObservableProperty] private decimal _priceSum;

    [ObservableProperty] string? _uniValue = string.Empty;
    private string? _textDecimal = "0,00";
    
    public string? TextDecimal
    {
        get => _textDecimal;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Price = 0m;
                _textDecimal = "0,00";
            }
            else
            {
                _textDecimal = value;
                _textDecimal = value.Replace('.', ',');
            }
        }

    }

    public int? Id { get; set; }
    public int ZakupkaId { get; set; }
    public int? MainCatId { get; set; }

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