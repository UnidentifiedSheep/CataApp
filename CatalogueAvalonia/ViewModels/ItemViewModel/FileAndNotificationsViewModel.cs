using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.ViewModels.ItemViewModel;

public class FileAndNotificationsViewModel : ViewModelBase
{
    private readonly Bitmap[] Icons =
    [
        new Bitmap(AssetLoader.Open(new Uri("avares://CatalogueAvalonia/Assets/Processing.png"))),
        new Bitmap(AssetLoader.Open(new Uri("avares://CatalogueAvalonia/Assets/Ready.png"))),
        new Bitmap(AssetLoader.Open(new Uri("avares://CatalogueAvalonia/Assets/NotAvailable.png")))
    ];
    
    private ObservableCollection<NotificationModel> _notification;
    public IEnumerable<NotificationModel> Notifications => _notification;
    public FileAndNotificationsViewModel()
    {
        _notification = new();
        Messenger.Register<AddedMessage>(this, FileAdded);
        Messenger.Register<EditedMessage>(this, FileEdited);
    }

    private void FileEdited(object recipient, EditedMessage message)
    {
        var where = message.Value.Where;
        if (where == "FileReady")
        {
            var model = _notification.FirstOrDefault(x => x.FileId == message.Value.Id);
            if (model != null)
                Dispatcher.UIThread.Post(() => model.Ico = Icons[(int)model.StatusOfFile]);
        }
        else if(where == "FailedToGenerate")
        {
            var model = _notification.FirstOrDefault(x => x.FileId == message.Value.Id);
            if(model != null)
                    Dispatcher.UIThread.Post(() =>
                    {
                        model.StatusOfFile = FileStatus.NotAvailable;
                        model.Ico = Icons[2];
                        model.StepsState = "Файл не доступен.";
                    });
        }
    }
    
    public async Task ChangeStateAndImg(int id, FileStatus state)
    {
        if (state == FileStatus.NotAvailable)
        {
            var parent = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
            var model = _notification.First(x => x.FileId == id);
            model.StatusOfFile = state;
            model.Ico = Icons[2];
            model.StepsState = "Файл не доступен.";
            await MessageBoxManager.GetMessageBoxStandard("!",
                $"Не удалось найти укзанный файл.").ShowWindowDialogAsync(parent);
        }
    }

    private void FileAdded(object recipient, AddedMessage message)
    {
        var where = message.Value.Where;
        if (where == "FileAdded")
        {
            var what = (NotificationModel)message.Value.What!;
            Dispatcher.UIThread.Post(() =>
            {
                what.Ico = Icons[(int)what.StatusOfFile];
                if (what.FileInfo.Length >= 45)
                    what.FileInfo = what.FileInfo.Substring(0,45) + "...";
                _notification.Insert(0,what);
            });
            
        }
    }

    public FileAndNotificationsViewModel(IMessenger messenger) : base(messenger)
    {
        Messenger.Register<AddedMessage>(this, FileAdded);
        Messenger.Register<EditedMessage>(this, FileEdited);
        _notification = new();
    }
}