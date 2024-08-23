
namespace DataBase.Data;

public partial class Prodaja
{
    public int Id { get; set; }

    public int ProdajaId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public int? MainCatId { get; set; }

    public string? UniValue { get; set; }

    public string? MainName { get; set; }

    public decimal InitialPrice { get; set; }

    public int CurrencyId { get; set; }
    public string? Comment { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual MainCat? MainCat { get; set; }

    public virtual ProdMainGroup ProdajaNavigation { get; set; } = null!;
}
