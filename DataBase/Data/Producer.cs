
namespace DataBase.Data;

public partial class Producer
{
    public int Id { get; set; }

    public string ProducerName { get; set; } = null!;

    public virtual ICollection<MainCat> MainCats { get; set; } = new List<MainCat>();
}
