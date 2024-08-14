

namespace DataBase.Data;

public partial class Agent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int IsZak { get; set; }
    public int OverPr { get; set; }

    public virtual ICollection<AgentTransaction> AgentTransactions { get; set; } = new List<AgentTransaction>();

    public virtual ICollection<ProdMainGroup> ProdMainGroups { get; set; } = new List<ProdMainGroup>();

    public virtual ICollection<ZakMainGroup> ZakMainGroups { get; set; } = new List<ZakMainGroup>();
}
