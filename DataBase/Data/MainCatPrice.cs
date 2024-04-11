using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class MainCatPrice
{
    public int Id { get; set; }

    public int MainCatId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual MainCat MainCat { get; set; } = null!;
}
