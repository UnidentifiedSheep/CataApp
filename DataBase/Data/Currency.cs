using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class Currency
{
    public int Id { get; set; }

    public string CurrencyName { get; set; } = null!;

    public double ToUsd { get; set; }

    public virtual ICollection<AgentTransaction> AgentTransactions { get; set; } = new List<AgentTransaction>();

    public virtual ICollection<MainCatPrice> MainCatPrices { get; set; } = new List<MainCatPrice>();

    public virtual ICollection<ProdMainGroup> ProdMainGroups { get; set; } = new List<ProdMainGroup>();

    public virtual ICollection<ZakMainGroup> ZakMainGroups { get; set; } = new List<ZakMainGroup>();
}
