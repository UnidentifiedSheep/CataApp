using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class AddNewTransactionWindow : Window
{
    public AddNewTransactionWindow()
    {
        InitializeComponent();
    }

    public async void SaveButt_Click(object sender, RoutedEventArgs e)
    {
        var dc = (AddNewTransactionViewModel?)DataContext;
        if (dc != null)
        {
            if (dc.TransactionSum != 0)
            {
                if (dc.SelectedCurrency != null)
                {
                    Close();
                    dc.AddTransactionCommand.Execute(null);
                }
                else
                {
                    await MessageBoxManager.GetMessageBoxStandard("?",
                        "�� �� ������� ������.").ShowWindowDialogAsync(this);
                }
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("?",
                    "����� ����� 0.").ShowWindowDialogAsync(this);
            }
        }
    }

    public void CancleButt_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}