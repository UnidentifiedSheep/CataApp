namespace CatalogueAvalonia.Models;

public class AgentTransactionModel
{
    private string _currencySign = string.Empty;
    private decimal _transactionSum;
    public int Id { get; set; }
    public int AgentId { get; set; }
    public int TransactionStatus { get; set; }

    public decimal TransactionSum
    {
        get => _transactionSum;
        set
        {
            _transactionSum = value;
            if (TransactionStatus == 0 || TransactionStatus == 2)
                Summa = value;
            else if (TransactionStatus == 1 || TransactionStatus == 4)
                SummaPlateja = value;
        }
    }

    public decimal Balance { get; set; }
    public decimal Summa { get; set; }
    public decimal SummaPlateja { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;

    public string CurrencySign
    {
        get => _currencySign;
        set
        {
            if (!string.IsNullOrEmpty(value) || value == " ")
                _currencySign = value;
            else
                _currencySign = CurrencyName[..3];
        }
    }

    public string TransactionDatatime { get; set; } = string.Empty;
}