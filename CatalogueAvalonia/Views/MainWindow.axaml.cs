using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;

    }

    private bool _canClose = false;
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true;
        var res = await MessageBoxManager
            .GetMessageBoxStandard("Сохранить?", "Желаете ли вы сохранить копию базы данных?", ButtonEnum.YesNo)
            .ShowWindowDialogAsync(this);
        if (res == ButtonResult.Yes)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
            var filePath = Directory.GetCurrentDirectory() /*.Replace("\\bin", "")*/.Replace("\\net8.0", "") + "\\Data\\data.db";
            var folderPath = folder[0].Path.ToString().Replace("file:///", "") + $"\\data({DateTime.Now:dd.MM.yyyy}).db";
            File.Copy(filePath, folderPath);
        }
        _canClose = true;
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        var dc = (MainWindowViewModel?)DataContext;
        if (dc != null)
        {
            IMessenger messenger = dc.GetMessenger();
            messenger.Send(new ActionMessage("AppClosed"));
        }
    }
    public async Task StartTransitionUpAsync()
    {
        var animation = (Animation)Resources["TransitionUpAnimation"];
        await animation.RunAsync(SearchGrid);
    }
    public async Task StartTransitionDownAsync()
    {
        var animation = (Animation)Resources["TransitionDownAnimation"];
        await animation.RunAsync(SearchGrid);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var dc = (MainWindowViewModel?)DataContext;
        if (dc != null)
        {
            dc.SetTextBoxVisOrUnvisCommand.Execute(null);
        }
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var dc = (MainWindowViewModel?)DataContext;
        dc?.ChangeUniValuesVis();
    }
}