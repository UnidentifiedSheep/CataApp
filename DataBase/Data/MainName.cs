namespace DataBase.Data;

public class MainName
{
    public int UniId { get; set; }

    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public virtual ICollection<MainCat> MainCats { get; set; } = new List<MainCat>();
}