using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class AgentBalance
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public int AgentId { get; set; }

    public decimal Balance { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;
}
