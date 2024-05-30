
namespace DataBase.Data;

public partial class MainCat
{
    public int Id { get; set; }

    public int UniId { get; set; }

    public string UniValue { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int ProducerId { get; set; }

    public int Count { get; set; }

    public byte[]? Img { get; set; }

    public string RowColor { get; set; } = null!;

    public virtual ICollection<MainCatPrice> MainCatPrices { get; set; } = new List<MainCatPrice>();

    public virtual ICollection<PartInGroup> PartInGroups { get; set; } = new List<PartInGroup>();

    public virtual ICollection<Prodaja> Prodajas { get; set; } = new List<Prodaja>();

    public virtual Producer Producer { get; set; } = null!;

    public virtual MainName Uni { get; set; } = null!;

    public virtual ICollection<ZakProdCount> ZakProdCounts { get; set; } = new List<ZakProdCount>();

    public virtual ICollection<Zakupka> Zakupkas { get; set; } = new List<Zakupka>();
}
