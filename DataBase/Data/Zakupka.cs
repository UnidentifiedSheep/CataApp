
namespace DataBase.Data;

public partial class Zakupka
{
    public int Id { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public int ZakId { get; set; }

    public int? MainCatId { get; set; }

    public string? UniValue { get; set; }

    public string? MainName { get; set; }

    public virtual MainCat? MainCat { get; set; }

    public virtual ZakMainGroup Zak { get; set; } = null!;
}
