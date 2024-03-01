using System;
using System.Collections.Generic;

namespace DataBase.Data;

public partial class MainCat
{
    public int Id { get; set; }

    public int UniId { get; set; }

    public string UniValue { get; set; } = null!;
    private string _name = string.Empty;
    public string Name
    {
        get
        {
            return _name;
        }
        set 
        { 
            if (value == "Название не указано") 
                _name = string.Empty;

            else
                _name = value; 
        }
    }

    public int ProducerId { get; set; }

    public int Count { get; set; }

    public virtual ICollection<MainCatPrice> MainCatPrices { get; set; } = new List<MainCatPrice>();

    public virtual ICollection<Prodaja> Prodajas { get; set; } = new List<Prodaja>();

    public virtual Producer Producer { get; set; } = null!;

    public virtual MainName Uni { get; set; } = null!;

    public virtual ICollection<Zakupka> Zakupkas { get; set; } = new List<Zakupka>();
}
