namespace CatalogueAvalonia.Models;

public class CurrencyModel
{
    private string _currencySign = string.Empty;
    private decimal _toUsd;


    public bool IsDirty;
    public int? Id { get; set; } = null;
    public string CurrencyName { get; set; } = string.Empty;

    public decimal ToUsd
    {
        get => _toUsd;
        set
        {
            _toUsd = value;
            IsDirty = true;
        }
    }

    public int CanDelete { get; set; }

    public string CurrencySign
    {
        get => _currencySign;
        set
        {
            if (string.IsNullOrEmpty(value) || value == " ")
                _currencySign = value.Substring(0, 3) + '.';
            else
                _currencySign = value;
        }
    }
}