using Avalonia.Controls;
using Avalonia.Input;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.ViewModels;

namespace CatalogueAvalonia.Views;

public partial class CatalogueView : UserControl
{
    
    public CatalogueView()
    {
        InitializeComponent();
    }


    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var dc = (CatalogueViewModel?)DataContext;
        var dataGrid = (TreeDataGrid?)sender;
        if (dataGrid != null && dc != null)
        {
            if (dataGrid.RowSelection != null)
            {
                dc.ExpandRow(dataGrid.RowSelection.SelectedIndex);
            }
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var dc = (CatalogueViewModel?)DataContext;
        var dataGrid = (TreeDataGrid?)sender;
        if (dataGrid != null && dc != null)
        {
            if (e.Key == Key.Return)
            {
                if (dataGrid.RowSelection != null)
                {
                    dc.ExpandRow(dataGrid.RowSelection.SelectedIndex);
                }
            }    
        }
    }
}