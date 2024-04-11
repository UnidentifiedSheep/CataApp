using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class MainName
{
    public int UniId { get; set; }

    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public virtual ICollection<MainCat> MainCats { get; set; } = new List<MainCat>();
}
