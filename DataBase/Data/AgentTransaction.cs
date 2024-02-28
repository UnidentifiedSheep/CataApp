﻿using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class AgentTransaction
{
    public int Id { get; set; }

    public int AgentId { get; set; }

    public int TransactionStatus { get; set; }

    public double TransactionSum { get; set; }

    public double Balance { get; set; }

    public string TransactionDatatime { get; set; } = null!;

    public int Currency { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Currency CurrencyNavigation { get; set; } = null!;
}
