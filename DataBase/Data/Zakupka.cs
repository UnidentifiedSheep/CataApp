using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class Zakupka
{
    public int Id { get; set; }

    public int Count { get; set; }

    public double Price { get; set; }

    public int MainCatId { get; set; }

    public int ZakId { get; set; }

    public virtual MainCat MainCat { get; set; } = null!;

    public virtual ZakMainGroup Zak { get; set; } = null!;
}
