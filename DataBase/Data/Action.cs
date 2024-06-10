
namespace DataBase.Data;

public partial class Action
{
    public int Id { get; set; }

    public int Action1 { get; set; }

    public string Description { get; set; } = null!;

    public string Values { get; set; } = null!;

    public int Seen { get; set; }

    public string? Comment { get; set; }
    public string Date { get; set; } = null!;
    public string Time { get; set; } = null!;
}
