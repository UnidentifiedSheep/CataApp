﻿using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class PartInGroup
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int MainCatId { get; set; }

    public virtual PartsGroup Group { get; set; } = null!;

    public virtual MainCat MainCat { get; set; } = null!;
}
