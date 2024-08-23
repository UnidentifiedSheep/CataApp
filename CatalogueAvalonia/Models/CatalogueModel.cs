using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;

namespace CatalogueAvalonia.Models;

public partial class CatalogueModel : ObservableObject
{
    [ObservableProperty] private int _count;

    [ObservableProperty] private string _currencyName = string.Empty;

    [ObservableProperty] private string _name = "Название не указано";

    [ObservableProperty] private decimal? _price;

    [ObservableProperty] private string _producerName = string.Empty;

    [ObservableProperty] private string _uniValue = string.Empty;

    [ObservableProperty] private decimal? _visiblePrice;
    [ObservableProperty] private string _rowColor = "#FFFFFF";
    [ObservableProperty] private string _textColor = "#000000";

    public int? UniId { get; set; }
    public int PriceId { get; set; }
    public int? MainCatId { get; set; }
    public int ProducerId { get; set; }
    public int CurrencyId { get; set; }

    public ObservableCollection<CatalogueModel>? Children { get; set; }
    public ObservableCollection<CatalogueModel> UnVisChildren { get; private set; } = new();
    

    partial void OnNameChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || value == " " || value == "  ")
            Name = "Название не указано";
    }

    partial void OnPriceChanged(decimal? value)
    {
        if (value != null)
        {
            VisiblePrice = Math.Round(value ?? 0, 2);
        }
        else
        {
            VisiblePrice = null;
        }
    }
}
