using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
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
using Newtonsoft.Json;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewPartViewModel : ViewModelBase
{
    private readonly ObservableCollection<CatalogueModel> _catalogueModels;
    private readonly DataStore _dataStore;
    private readonly ObservableCollection<ProducerModel> _producers;
    private readonly TopModel _topModel;
    private readonly IDialogueService _dialogueService;

    [ObservableProperty] private string _nameOfParts = string.Empty;

    [ObservableProperty] private string _parts = string.Empty;

    [ObservableProperty] private string _producerSearchField = string.Empty;

    [ObservableProperty] private CatalogueModel? _selectedCatalogue;

    [ObservableProperty] private ProducerModel? _selectedProducer;

    private List<string> parts = new();

    public AddNewPartViewModel()
    {
        _catalogueModels = new ObservableCollection<CatalogueModel>();
        _producers = new ObservableCollection<ProducerModel>();
    }

    public AddNewPartViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _catalogueModels = new ObservableCollection<CatalogueModel>();
        _producers = new ObservableCollection<ProducerModel>(_dataStore.ProducerModels);
        _dialogueService = dialogueService;
        Messenger.Register<AddedMessage>(this, OnProducerAdded);
    }

    private void OnProducerAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Producer")
            _producers.Add((ProducerModel)message.Value.What!);
    }

    public IEnumerable<CatalogueModel> Catalogues => _catalogueModels;
    public IEnumerable<ProducerModel> Producers => _producers;

    partial void OnPartsChanged(string value)
    {
        var filterSlashes = new AsyncRelayCommand(async () => { parts = await DataFiltering.FilterSlashes(value); });
        
        if (value.Length >= 2)
        {
            parts.Clear();
            _catalogueModels.Clear();
            filterSlashes.Execute(null);
            _catalogueModels.AddRange(parts.Select(x => new CatalogueModel
            {
                UniValue = x,
                ProducerId = 1,
                ProducerName = "Неизвестный"
            }));
        }
        else
        {
            _catalogueModels.Clear();
        }
    }
    

    [RelayCommand]
    private async Task filterProducers(string value)
    {
        await foreach (var item in DataFiltering.FilterProducer(_dataStore.ProducerModels, value))
            _producers.Add(item);
    }

    partial void OnProducerSearchFieldChanged(string value)
    {
        _producers.Clear();
        filterProducersCommand.Execute(value);
    }

    partial void OnSelectedCatalogueChanged(CatalogueModel? value)
    {
        if (value != null && SelectedProducer != null)
        {
            var index = _catalogueModels.IndexOf(value);
            ChangeProducer(index, SelectedProducer.Id, SelectedProducer.ProducerName);
        }
    }

    partial void OnSelectedProducerChanged(ProducerModel? value)
    {
        if (value != null && SelectedCatalogue != null)
        {
            var index = _catalogueModels.IndexOf(SelectedCatalogue);
            ChangeProducer(index, value.Id, value.ProducerName);
        }
    }

    private void ChangeProducer(int index, int id, string name)
    {
        if (SelectedCatalogue != null && SelectedProducer != null)
        {
            SelectedCatalogue = null;
            SelectedProducer = null;
            _catalogueModels[index].ProducerId = id;
            _catalogueModels[index].ProducerName = name;
        }
    }

    [RelayCommand]
    private async Task AddToCatalogue()
    {
        var newId = await _topModel.AddNewCatalogue(new CatalogueModel
        {
            Name = NameOfParts,
            Children = new ObservableCollection<CatalogueModel>(_catalogueModels)
        });

        Messenger.Send(new AddedMessage(new ChangedItem
            { Id = newId, Where = "Catalogue", What = await _topModel.GetCatalogueByIdAsync(newId) }));
    }

    [RelayCommand]
    private async Task AddNewProducer(Window parent)
    {
        await _dialogueService.OpenDialogue(new AddNewProducerWindow(), new AddNewProducerViewModel(Messenger, _topModel), parent);
    }
}