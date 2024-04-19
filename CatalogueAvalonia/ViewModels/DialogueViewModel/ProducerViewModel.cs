using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class ProducerViewModel : ViewModelBase
{
    private readonly DataStore _dataStore;
    private readonly TopModel _topModel;
    private readonly IDialogueService _dialogueService;
    [ObservableProperty] private ProducerModel? _selectedProducer;
    [ObservableProperty] private string _searchField = string.Empty;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProducerCommand))] 
    [NotifyCanExecuteChangedFor(nameof(EditProducerNameCommand))]  [NotifyCanExecuteChangedFor(nameof(CancleCommand))]
    private bool _actionEnded = true;

    private readonly ObservableCollection<ProducerModel> _producers;
    public IEnumerable<ProducerModel> Producers => _producers;
    
    public ProducerViewModel()
    {
        _producers = new ObservableCollection<ProducerModel>();
    }
    public ProducerViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        _producers = new ObservableCollection<ProducerModel>(dataStore.ProducerModels.Where(x => x.Id != 1));
        
        Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
    }

    private void OnDataBaseAdded(object recipient, AddedMessage message)
    {
        if(message.Value.Where == "Producer")
        {
            var what = (ProducerModel?)message.Value.What;
            if (what != null)
                _producers.Add(what);
        }
    }

    [RelayCommand]
    public async Task FilterProducer(string objToFind)
    {
        _producers.Clear();
        await foreach (var item in DataFiltering.FilterProducer(_dataStore.ProducerModels.Where(x => x.Id != 1),
                           objToFind))
            _producers.Add(item);
    }
    partial void OnSearchFieldChanged(string value)
    {
        if (value.Length >= 1)
        {
            FilterProducerCommand.Execute(value);
        }
        else
        {
            if (_dataStore.ProducerModels.Count(x => x.Id != 1) != _producers.Count)
            {
                _producers.Clear();
                _producers.AddRange(_dataStore.ProducerModels.Where(x => x.Id != 1));
            }
        }
    }

    [RelayCommand(CanExecute = nameof(ActionEnded))]
    private async Task AddProducer(Window parent)
    {
        await _dialogueService.OpenDialogue(new AddNewProducerWindow(),
            new AddNewProducerViewModel(Messenger, _topModel), parent);
    }

    [RelayCommand(CanExecute = nameof(ActionEnded))]
    private async Task EditProducerName(ProducerModel producer)
    {
        await _topModel.EditProducerAsync(producer.Id, producer.ProducerName);
        Messenger.Send(new EditedMessage(new ChangedItem
            { Id = producer.Id, MainName = producer.ProducerName, Where = "Producer" }));
    }
    
    [RelayCommand(CanExecute = nameof(ActionEnded))]
    private async Task DeleteProducer(Window parent)
    {
        if (SelectedProducer != null)
        {
            var res = await MessageBoxManager.GetMessageBoxStandard("Удалить Производителя",
                $"Вы уверенны что хотите удалить: \"{SelectedProducer.ProducerName}\"?" +
                $"\nПосле удаления производителя, запчасти от данного производителя станут с неизвестным производителем.",
                ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
            if (res == ButtonResult.Yes)
            {
                ActionEnded = false;
                var actionResult = await _topModel.DeleteProducer(SelectedProducer.Id);
                Messenger.Send(new DeletedMessage(new DeletedItem { Id = SelectedProducer.Id, Where = "Producer" }));
                _producers.Remove(SelectedProducer);
                
                ActionEnded = true;
                if (!actionResult)
                {
                    await MessageBoxManager.GetMessageBoxStandard("!",
                        $"Не удалось удалить производителя.").ShowWindowDialogAsync(parent);
                }
            }
        }
    }

    [RelayCommand(CanExecute = nameof(ActionEnded))]
    private void Cancle(Window parent)
    {
        parent.Close();
    }
}