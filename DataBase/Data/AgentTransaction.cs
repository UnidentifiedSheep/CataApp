using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class AgentTransaction
{
    public int Id { get; set; }

    public int AgentId { get; set; }

    public int TransactionStatus { get; set; }

    public decimal TransactionSum { get; set; }

    public decimal Balance { get; set; }

    public string TransactionDatatime { get; set; } = null!;

    public int Currency { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Currency CurrencyNavigation { get; set; } = null!;

    public virtual ICollection<ProdMainGroup> ProdMainGroups { get; set; } = new List<ProdMainGroup>();

    public virtual ICollection<ZakMainGroup> ZakMainGroups { get; set; } = new List<ZakMainGroup>();
}
