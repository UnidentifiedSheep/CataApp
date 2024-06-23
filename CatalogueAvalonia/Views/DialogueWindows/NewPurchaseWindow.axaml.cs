using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class NewPurchaseWindow : Window
{
    private readonly string _viewModelName;

    public NewPurchaseWindow()
    {
        InitializeComponent();
    }

    public NewPurchaseWindow(string viewModelName)
    {
        InitializeComponent();
        _viewModelName = viewModelName;
    }

    private async void SaveButt_Clicked(object sender, RoutedEventArgs e)
    {
        EditPurchaseViewModel? dcE = null;
        NewPurchaseViewModel? dcN = null;
        if (_viewModelName == "NewPurchaseViewModel")
            dcN = (NewPurchaseViewModel?)DataContext;
        else if (_viewModelName == "EditPurchaseViewModel")
            dcE = (EditPurchaseViewModel?)DataContext;

        if (dcN != null)
        {
            if (dcN.Zakupka.Any())
            {
                var whereZero = dcN.Zakupka.Where(x => x.Count <= 0 || x.Price <= 0.0099m || x.Count == null || x.Price == null);
                if (!whereZero.Any())
                {
                    if (dcN.SelectedAgent != null)
                    {
                        if (dcN.SelectedCurrency != null)
                            dcN.SaveZakupkaCommand.Execute(this);
                        else
                            await MessageBoxManager.GetMessageBoxStandard("?",
                                "Вы не выбрали валюту.").ShowWindowDialogAsync(this);
                    }
                    else
                    {
                        await MessageBoxManager.GetMessageBoxStandard("?",
                            "Вы не выбрали контрагента.").ShowWindowDialogAsync(this);
                    }
                }
                else
                {
                    var res = await MessageBoxManager.GetMessageBoxStandard("?",
                        "Есть позиции у которых Цена либо Количество = 0.\n Удалить их и продолжить закупку?",
                        ButtonEnum.YesNo).ShowWindowDialogAsync(this);
                    if (res == ButtonResult.Yes)
                        dcN.RemoveWhereZero(whereZero);
                }
            }
            else
            {
                Close();
            }
        }
        else if (dcE != null)
        {
            if (dcE.Zakupka.Any())
            {
                if (dcE.IsDirty)
                {
                    var whereZero = dcE.Zakupka.Where(x => x.Count <= 0 || x.Price <= 0.0099m || x.Count == null || x.Price == null);
                    if (!whereZero.Any())
                    {
                        if (dcE.SelectedAgent != null)
                        {
                            if (dcE.SelectedCurrency != null)
                            {
                                Close();
                                dcE.SaveZakupkaCommand.Execute(null);
                            }
                            else
                            {
                                await MessageBoxManager.GetMessageBoxStandard("?",
                                    "Вы не выбрали валюту.").ShowWindowDialogAsync(this);
                            }
                        }
                        else
                        {
                            await MessageBoxManager.GetMessageBoxStandard("?",
                                "Вы не выбрали контрагента.").ShowWindowDialogAsync(this);
                        }
                    }
                    else
                    {
                        var res = await MessageBoxManager.GetMessageBoxStandard("?",
                            "Есть позиции у которых Цена либо Количество = 0.\n Удалить их и продолжить закупку?",
                            ButtonEnum.YesNo).ShowWindowDialogAsync(this);
                        if (res == ButtonResult.Yes)
                            dcE.RemoveWhereZero(whereZero);
                    }
                }
                else
                {
                    Close();
                }
            }
            else
            {
                var res = await MessageBoxManager.GetMessageBoxStandard("Удалить закупку?",
                    "Вы уверенны что хотите удалить закупку?",
                    ButtonEnum.YesNo).ShowWindowDialogAsync(this);
                if (res == ButtonResult.Yes)
                {
                    dcE.DeleteAllCommand.Execute(null);
                    Close();
                }
            }
        }
    }

    private void CancleButt_Clicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        EditPurchaseViewModel? dcE = null;
        NewPurchaseViewModel? dcN = null;
        if (_viewModelName == "NewPurchaseViewModel")
            dcN = (NewPurchaseViewModel?)DataContext;
        else if (_viewModelName == "EditPurchaseViewModel")
            dcE = (EditPurchaseViewModel?)DataContext;

        if (dcN != null)
            dcN.TotalSum = dcN.Zakupka.Sum(x => x.PriceSum);
        else if (dcE != null)
        {
            dcE.TotalSum = dcE.Zakupka.Sum(x => x.PriceSum);
            dcE.IsDirty = true;
        }
        
    }
}