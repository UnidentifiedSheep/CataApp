using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class AddNewAgentWindow : Window
{
    public AddNewAgentWindow()
    {
        InitializeComponent();
    }

    public void Cancle_ButtClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public async void Save_ButtClicked(object sender, RoutedEventArgs e)
    {
        var dc = (AddNewAgentViewModel?)DataContext;
        if (dc != null)
        {
            if (!string.IsNullOrEmpty(dc.AgentName))
            {
                Close();
                dc.AddNewAgentCommand.Execute(null);
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("?",
                    "Вы не ввели имя контрагента").ShowWindowDialogAsync(this);
            }
        }
    }
}