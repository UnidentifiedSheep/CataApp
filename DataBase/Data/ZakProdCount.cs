namespace DataBase.Data;

public partial class ZakProdCount
{
    public int Id { get; set; }

    public int MainCatId { get; set; }

    public int BuyCount { get; set; }

    public int SellCount { get; set; }

    public virtual MainCat MainCat { get; set; } = null!;
}
