using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CatalogueAvalonia.Services.BarcodeServer;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SQLitePCL;

namespace CatalogueAvalonia.ViewModels.ItemViewModel;

public partial class ServerStatusViewModel : ViewModelBase
{
    private readonly Listener _listener;
    [ObservableProperty] private string _statusColor = Colors.Red.ToString();
    [ObservableProperty] private string _status = "Оффлайн";
    [ObservableProperty] private bool _hasStarted = false;

    private ObservableCollection<string> _comments;
    public IEnumerable<string> Comments => _comments;

    public ServerStatusViewModel()
    {
    }
    public ServerStatusViewModel(IMessenger messenger, Listener listener) : base(messenger)
    {
        _listener = listener;
        _comments = new ObservableCollection<string>();
        Messenger.Register<ServerMessage>(this, ServerMessageIn);
    }

    private void ServerMessageIn(object recipient, ServerMessage message)
    {
        Dispatcher.UIThread.Post(() => _comments.Insert(0,message.Value));
    }

    [RelayCommand]
    private async Task StartListening()
    {
        await _listener.StartListener();
        HasStarted = true;
        StatusColor = Colors.Green.ToString();
        Status = "Онлайн";

    }

    [RelayCommand]
    private async Task StopListening()
    {
        await _listener.StopListener();
        HasStarted = false;
        StatusColor = Colors.Red.ToString();
        Status = "Оффлайн";
    }
}