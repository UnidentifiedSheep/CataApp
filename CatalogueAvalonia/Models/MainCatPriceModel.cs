using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class MainCatPriceModel : ObservableObject
{
    private int _count;
    private decimal _price;
    private CurrencyModel? _selectedCurrency;
    
    public int? Id { get; set; }

    public int MainCatId { get; set; }

    public int CurrencyId { get; set; }

    public decimal? Price
    {
        get => _price;
        set
        {
            _price = value ?? 0;
            IsDirty = true;
        }
    }

    public bool IsDirty { get; set; }

    public int? Count
    {
        get => _count;
        set
        {
            _count = value ?? 0;
            IsDirty = true;
        }
    }

    [ObservableProperty] private string _rowInt;
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private string _rowDecimal = string.Empty;

    partial void OnRowDecimalChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            RowDecimal = "0,00";
        
    }

    partial void OnRowIntChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            RowInt = "0";
        
    }

    public CurrencyModel? SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            _selectedCurrency = value;
            if (value != null)
                CurrencyId = value.Id ?? default;
            IsDirty = true;
        }
    }

    public ObservableCollection<CurrencyModel>? Currency { get; set; } = null;
}