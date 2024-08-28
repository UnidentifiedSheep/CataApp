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
using MsBox.Avalonia.Controls;
using MsBox.Avalonia.Dto;
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
    private bool _isSaving = false;
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true;
        var res = await MessageBoxManager
            .GetMessageBoxStandard("Сохранить?", "Желаете ли вы сохранить копию базы данных?", ButtonEnum.YesNo)
            .ShowWindowDialogAsync(this);
        if (res == ButtonResult.Yes)
        {
            _isSaving = true;
            bool fileSaved = false;
            int? startCount = null;
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
            var f = Directory.GetCurrentDirectory();
            var filePath = f.Substring(0, f.LastIndexOf('\\')) + @"\Data\data.db";
            var folderPath = folder[0].Path.ToString().Replace("file:///", "") +
                             $"\\{startCount}data({DateTime.Now:dd.MM.yyyy}).db";
            var msBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                WindowIcon = null,
                CanResize = false,
                ShowInCenter = true,
                ContentTitle = "Сохранение",
                ContentHeader = null,
                ContentMessage = "Идет сохранение пожалуйста подождите",
                Markdown = false,
                Width = 340,
                Height = 140,
                SizeToContent = SizeToContent.Manual,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SystemDecorations = SystemDecorations.BorderOnly,
                Topmost = false,
                HyperLinkParams = null,
                Icon = MsBox.Avalonia.Enums.Icon.Info,
                ButtonDefinitions = null,
            }).ShowWindowDialogAsync(this);
            
            
            while (!fileSaved)
            {
                try
                {
                    await Task.Run(() => File.Copy(filePath, folderPath));
                    fileSaved = true;
                    if (startCount != null)
                        await MessageBoxManager
                            .GetMessageBoxStandard("Ok", $"Файл сохранен под название - {startCount}data({DateTime.Now:dd.MM.yyyy}).db")
                            .ShowWindowDialogAsync(this);
                }
                catch (Exception exception)
                {
                    if (exception.Message.Contains("already exists"))
                    {
                        if (startCount == null) startCount = 1;
                        else startCount++;
                        folderPath = folder[0].Path.ToString().Replace("file:///", "") +
                                     $"\\{startCount}data({DateTime.Now:dd.MM.yyyy}).db";
                    }
                }
            }
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
            messenger.Send(new ActionMessage(new ActionM("AppClosed")));
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