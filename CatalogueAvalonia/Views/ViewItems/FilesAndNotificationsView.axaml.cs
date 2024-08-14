using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.ViewModels.ItemViewModel;
using MsBox.Avalonia;

namespace CatalogueAvalonia.Views.ViewItems;

public partial class FilesAndNotificationsView : UserControl
{
    public FilesAndNotificationsView()
    {
        InitializeComponent();
    }

    private async void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var ls = (ListBox?)sender;
        var parent = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
        if (ls != null)
        {
            var dc = (FileAndNotificationsViewModel?)DataContext;
            var selectedItem = (NotificationModel?)ls.SelectedItem;
            if (selectedItem != null && !string.IsNullOrEmpty(selectedItem.FilePath) && selectedItem.StatusOfFile == FileStatus.Ready)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(selectedItem.FilePath) { UseShellExecute = true });
                }
                catch (Exception exception)
                {
                    if (exception.ToString().Contains("Не удается найти указанный файл") && dc != null)
                        await dc.ChangeStateAndImg(selectedItem.FileId, FileStatus.NotAvailable);
                }
            }
            else if (selectedItem != null && !string.IsNullOrEmpty(selectedItem.FilePath) &&
                     selectedItem.StatusOfFile == FileStatus.NotAvailable)
            {
                await MessageBoxManager.GetMessageBoxStandard("!",
                    $"Документ не был найден.").ShowWindowDialogAsync(parent);
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("!",
                    $"Документ пока что не готов подождите.").ShowWindowDialogAsync(parent);
            }
            
        }
    }
}