
namespace DataBase.Data;

public partial class Currency
{
    public int Id { get; set; }

    public string CurrencyName { get; set; } = null!;

    public string CurrencySign { get; set; } = null!;

    public decimal ToUsd { get; set; }

    public int CanDelete { get; set; }
    public virtual ICollection<AgentBalance> AgentBalances { get; set; } = new List<AgentBalance>();

    public virtual ICollection<AgentTransaction> AgentTransactions { get; set; } = new List<AgentTransaction>();

    public virtual ICollection<MainCatPrice> MainCatPrices { get; set; } = new List<MainCatPrice>();

    public virtual ICollection<ProdMainGroup> ProdMainGroups { get; set; } = new List<ProdMainGroup>();

    public virtual ICollection<Prodaja> Prodajas { get; set; } = new List<Prodaja>();

    public virtual ICollection<ZakMainGroup> ZakMainGroups { get; set; } = new List<ZakMainGroup>();
}
