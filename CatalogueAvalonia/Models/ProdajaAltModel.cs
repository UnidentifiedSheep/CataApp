using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ProdajaAltModel : ObservableObject
{
    [ObservableProperty] private int _count;

    [ObservableProperty] private int _currencyInitialId;

    [ObservableProperty] private decimal _initialPrice;

    [ObservableProperty] private string? _mainCatName = string.Empty;

    [ObservableProperty] private string? _mainName = string.Empty;

    [ObservableProperty] private int _maxCount;

    [ObservableProperty] private decimal _price;

    [ObservableProperty] private decimal _priceSum;

    [ObservableProperty] private string _producerName = string.Empty;

    [ObservableProperty] private string? _uniValue = string.Empty;

    public int? Id { get; set; }
    public int ProdajaId { get; set; }
    public int? MainCatId { get; set; }

    partial void OnPriceChanged(decimal value)
    {
        PriceSum = Math.Round(Price * Count, 2);
    }

    partial void OnCountChanged(int value)
    {
        PriceSum = Math.Round(Price * Count, 2);
    }
}