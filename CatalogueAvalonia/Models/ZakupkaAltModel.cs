using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ZakupkaAltModel : ObservableObject
{
    [ObservableProperty] private bool _canDelete = true;

    [ObservableProperty] public int _count;

    [ObservableProperty] public string? _mainCatName = string.Empty;

    [ObservableProperty] public string? _mainName = string.Empty;

    [ObservableProperty] private int _minCount;

    [ObservableProperty] public decimal _price;

    [ObservableProperty] private decimal _priceSum;

    [ObservableProperty] public string? _uniValue = string.Empty;

    public int? Id { get; set; }
    public int ZakupkaId { get; set; }
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