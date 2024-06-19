using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ProdajaAltModel : ObservableObject
{
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
    public int ProdajaId { get; set; }
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