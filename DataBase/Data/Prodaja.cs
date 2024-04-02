using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class Prodaja
{
    public int Id { get; set; }

    public int ProdajaId { get; set; }

    public double Price { get; set; }

    public int Count { get; set; }

    public int? MainCatId { get; set; }

    public string? UniValue { get; set; }

    public string? MainName { get; set; }

    public virtual MainCat? MainCat { get; set; }

    public virtual ProdMainGroup ProdajaNavigation { get; set; } = null!;
}
