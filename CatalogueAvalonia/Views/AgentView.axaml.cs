using Avalonia.Controls;
using CatalogueAvalonia.ViewModels;

namespace CatalogueAvalonia.Views;

public partial class AgentView : UserControl
{
    public AgentView()
    {
        InitializeComponent();
    }

    public void Agents_CellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
    {
        var dc = (AgentViewModel?)DataContext;

        if (dc != null) dc.EditAgentCommand.Execute(null);
    }
}