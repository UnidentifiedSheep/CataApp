using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class ProdMainGroup
{
    public int Id { get; set; }

    public string Datetime { get; set; } = null!;

    public double TotalSum { get; set; }

    public int AgentId { get; set; }

    public int CurrencyId { get; set; }

    public int TransactionId { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<Prodaja> Prodajas { get; set; } = new List<Prodaja>();

    public virtual AgentTransaction Transaction { get; set; } = null!;
}
