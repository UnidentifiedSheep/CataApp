
namespace DataBase.Data;

public partial class PartsGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public virtual ICollection<PartInGroup> PartInGroups { get; set; } = new List<PartInGroup>();
}
