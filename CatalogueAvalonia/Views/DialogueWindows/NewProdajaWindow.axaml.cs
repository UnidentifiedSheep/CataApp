using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class NewProdajaWindow : Window
{
    private readonly string _viewModelName;

    public NewProdajaWindow()
    {
        InitializeComponent();
    }

    public NewProdajaWindow(string viewModelName)
    {
        InitializeComponent();
        _viewModelName = viewModelName;
    }

    private void NumericUpDownPrice_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        EditProdajaViewModel? dcE = null;
        NewProdajaViewModel? dcN = null;
        if (_viewModelName == "NewProdajaViewModel")
            dcN = (NewProdajaViewModel?)DataContext;
        else if (_viewModelName == "EditProdajaViewModel")
            dcE = (EditProdajaViewModel?)DataContext;

        if (dcN != null)
        {
            if (e.NewValue != null)
                if (dcN.SelectedProdaja != null)
                {
                    dcN.SelectedProdaja.Price = (decimal)e.NewValue;
                    dcN.TotalSum = Math.Round(dcN.Prodaja.Sum(x => x.PriceSum), 2);
                }
        }
        else if (dcE != null)
        {
            if (e.NewValue != null)
                if (dcE.SelectedProdaja != null)
                {
                    dcE.IsDirty = true;
                    dcE.SelectedProdaja.Price = (decimal)e.NewValue;
                    dcE.TotalSum = Math.Round(dcE.Prodaja.Sum(x => x.PriceSum), 2);
                }
        }
    }

    private void NumericUpDownCount_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        EditProdajaViewModel? dcE = null;
        NewProdajaViewModel? dcN = null;
        if (_viewModelName == "NewProdajaViewModel")
            dcN = (NewProdajaViewModel?)DataContext;
        else if (_viewModelName == "EditProdajaViewModel")
            dcE = (EditProdajaViewModel?)DataContext;

        if (dcN != null)
        {
            if (e.NewValue != null)
                if (dcN.SelectedProdaja != null)
                {
                    dcN.SelectedProdaja!.Count = (int)e.NewValue;
                    dcN.TotalSum = Math.Round(dcN.Prodaja.Sum(x => x.PriceSum), 2);
                }
        }
        else if (dcE != null)
        {
            if (e.NewValue != null)
                if (dcE.SelectedProdaja != null)
                {
                    dcE.IsDirty = true;
                    dcE.SelectedProdaja!.Count = (int)e.NewValue;
                    dcE.TotalSum = Math.Round(dcE.Prodaja.Sum(x => x.PriceSum), 2);
                }
        }
    }

    private async void SaveButt_Clicked(object sender, RoutedEventArgs e)
    {
        EditProdajaViewModel? dcE = null;
        NewProdajaViewModel? dcN = null;
        if (_viewModelName == "NewProdajaViewModel")
            dcN = (NewProdajaViewModel?)DataContext;
        else if (_viewModelName == "EditProdajaViewModel")
            dcE = (EditProdajaViewModel?)DataContext;

        if (dcN != null)
        {
            if (dcN.Prodaja.Any())
            {
                var whereZero = dcN.Prodaja.Where(x => x.Count <= 0 || x.Price <= 0);
                if (!whereZero.Any())
                {
                    if (dcN.SelectedAgent != null)
                    {
                        if (dcN.SelectedCurrency != null)
                            dcN.SaveChangesCommand.Execute(this);
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
                await MessageBoxManager.GetMessageBoxStandard("?",
                    "Вы не добавили ни одной позиции.").ShowWindowDialogAsync(this);
            }
        }
        else if (dcE != null)
        {
            if (dcE.Prodaja.Any())
            {
                if (dcE.IsDirty)
                {
                    var whereZero = dcE.Prodaja.Where(x => x.Count <= 0 || x.Price <= 0);
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
}