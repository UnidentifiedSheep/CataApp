using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class EditCatalogueWindow : Window
{
    public EditCatalogueWindow()
    {
        InitializeComponent();
    }

    public void DataGrid_CellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
    {
        var dc = (EditCatalogueViewModel?)DataContext;

        if (dc != null) dc.IsDirty = true;
    }

    public async void SaveButt_Click(object sender, RoutedEventArgs e)
    {
        var dc = (EditCatalogueViewModel?)DataContext;
        if (dc != null)
        {
            if (dc.CurrAction == 0)
            {
                if (dc.IsDirty)
                {
                    if (dc.Catalogues.Any())
                    {
                        Close();
                        dc.SaveChangesCommand.Execute(null);
                    }
                    else
                    {
                        var res = await MessageBoxManager.GetMessageBoxStandard
                        (
                            "Список запчастей пуст",
                            "Вы уверены что хотите удалить группу запчастей?",
                            ButtonEnum.YesNo
                        ).ShowWindowDialogAsync(this);
                        if (res == ButtonResult.Yes)
                        {
                            Close();
                            dc.DeleteGroupCommand.Execute(null);
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
                else
                {
                    Close();
                }
            }
            else if (dc.CurrAction == 1)
            {
                if (dc.Catalogues.Any())
                {
                    if (!string.IsNullOrEmpty(dc.NameOfPart))
                    {
                        Close();
                        dc.AddToCatalogueCommand.Execute(null);
                    }
                    else
                    {
                        await MessageBoxManager.GetMessageBoxStandard
                        (
                            "?",
                            "Введите название."
                        ).ShowWindowDialogAsync(this);
                    }
                }
                else
                {
                    await MessageBoxManager.GetMessageBoxStandard
                    (
                        "?",
                        "Список запчастей пуст"
                    ).ShowWindowDialogAsync(this);
                }
            }
        }
    }

    public void CancleButt_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}