namespace DataBase.Data;

public partial class ZakMainGroup
{
    public int Id { get; set; }

    public string Datetime { get; set; } = null!;

    public double TotalSum { get; set; }

    public int AgentId { get; set; }

    public int CurrencyId { get; set; }

    public int TransactionId { get; set; }

    public string? Comment { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual AgentTransaction Transaction { get; set; } = null!;

    public virtual ICollection<Zakupka> Zakupkas { get; set; } = new List<Zakupka>();
}
