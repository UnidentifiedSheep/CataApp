using System.Collections.ObjectModel;

namespace CatalogueAvalonia.Models;

public class MainCatPriceModel
{
    private int _count;
    private decimal _price;
    private CurrencyModel? _selectedCurrency;
    public int? Id { get; set; }

    public int MainCatId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            IsDirty = true;
        }
    }

    public bool IsDirty { get; set; }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            IsDirty = true;
        }
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