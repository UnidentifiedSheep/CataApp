using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class ProducerWindow : Window
{
    public ProducerWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }
    ~ProducerWindow()
    {
        Closing -= OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        var dc = (ProducerViewModel?)DataContext;
        if (dc != null)
        {
            e.Cancel = !dc.ActionEnded;
        }
    }


    private async void DataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        var dc = (ProducerViewModel?)DataContext;
        if (dc != null)
        {
            
            var producer = (ProducerModel?)e.Row.DataContext;
            if (producer != null)
            {
                if (producer.ProducerName != string.Empty)
                {
                    if (producer.ProducerName != producer.StartName)
                    {
                        dc.EditProducerNameCommand.Execute(producer);
                        producer.StartName = producer.ProducerName;
                    }
                }
                else
                {
                    producer.ProducerName = producer.StartName;
                    await MessageBoxManager.GetMessageBoxStandard("?",
                        $"Имя производителя не содержит символов").ShowWindowDialogAsync(this);
                }
            }
        }
    }
    
}