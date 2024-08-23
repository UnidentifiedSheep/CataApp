using Avalonia.Controls;
using Avalonia.Input;
using CatalogueAvalonia.ViewModels.DialogueViewModel;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class CatalogueItemWindow : Window
{
    public CatalogueItemWindow()
    {
        InitializeComponent();
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var dc = (CatalogueItemViewModel?)DataContext;
        var dataGrid = (TreeDataGrid?)sender;
        if (dataGrid == null || dc == null) return;
        if (dataGrid.RowSelection != null)
            dc.ExpandRow(dataGrid.RowSelection.SelectedIndex);
        
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var dc = (CatalogueItemViewModel?)DataContext;
        var dataGrid = (TreeDataGrid?)sender;
        
        if (dataGrid == null || dc == null) return;
        if (e.Key != Key.Return) return;
        
        if (dataGrid.RowSelection != null)
            dc.ExpandRow(dataGrid.RowSelection.SelectedIndex);
        
    }
}