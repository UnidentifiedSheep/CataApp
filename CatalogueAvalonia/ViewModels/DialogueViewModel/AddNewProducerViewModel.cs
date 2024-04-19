using System.Threading.Tasks;
using Avalonia.Controls;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewProducerViewModel : ViewModelBase
{
    private readonly TopModel _topModel;
    [ObservableProperty] private string _producerName = string.Empty;
    public AddNewProducerViewModel()
    {
        
    }
    public AddNewProducerViewModel(IMessenger messenger, TopModel topModel) : base(messenger)
    {
        _topModel = topModel;
    }

    [RelayCommand]
    private async Task SaveProducer(Window parent)
    {
        if (ProducerName.Length >= 2)
        {
            var producer = await _topModel.AddNewProducer(ProducerName);
            Messenger.Send(new AddedMessage(new ChangedItem { What = producer, Where = "Producer" }));
            parent.Close();
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard("?",
                $"Имя производителя должно быть длиннее или равно 2 символам").ShowWindowDialogAsync(parent);
        }
    }

    [RelayCommand]
    private void Cancle(Window parent)
    {
        parent.Close();
    }
}