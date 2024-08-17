using System;
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

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class EditCatalogueViewModel : ViewModelBase
{
    private readonly ObservableCollection<CatalogueModel> _catalogueModels;
    private readonly IDialogueService _dialogueService;
    private readonly DataStore _dataStore;
    private readonly ObservableCollection<ProducerModel> _producers;
    private readonly TopModel _topModel;
    private readonly int? _uniId;
    private readonly int _currAction = 0;
    public int CurrAction => _currAction;

    [ObservableProperty] private string _nameOfPart = string.Empty;

    [ObservableProperty] private CatalogueModel? _selectedCatalogue;

    [ObservableProperty] private ProducerModel? _selectedProducer;
    [ObservableProperty] private string _producerSearch = String.Empty;

    private readonly List<int> ids = new();
    public bool IsDirty;

    public EditCatalogueViewModel()
    {
        _producers = new ObservableCollection<ProducerModel>();
        for (var i = 0; i < 10; i++)
            _producers.Add(new ProducerModel { Id = i, ProducerName = $"Producer{i}" });
        _catalogueModels = new ObservableCollection<CatalogueModel>();
        for (var i = 0; i < 10; i++)
            _catalogueModels.Add(new CatalogueModel { Name = "", UniValue = $"part{i}", ProducerName = $"sampa{i}" });
    }

    public EditCatalogueViewModel(IMessenger messenger, DataStore dataStore, int? id, TopModel topModel, IDialogueService dialogueService) :
        base(messenger)
    {
        _topModel = topModel;
        _uniId = id;
        _dataStore = dataStore;
        _producers = new ObservableCollection<ProducerModel>(_dataStore.ProducerModels);
        _catalogueModels = new ObservableCollection<CatalogueModel>();
        _currAction = 0;
        _dialogueService = dialogueService;
        GetParts(_uniId);
        Messenger.Register<AddedMessage>(this, OnProducerAdded);
    }

    public EditCatalogueViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IEnumerable<CatalogueModel> models, IDialogueService dialogueService) : base(messenger)
    {
        _topModel = topModel;
        _dataStore = dataStore;
        _producers = new ObservableCollection<ProducerModel>(_dataStore.ProducerModels);
        _catalogueModels = new ObservableCollection<CatalogueModel>(models);
        _currAction = 1;
        _dialogueService = dialogueService;
        Messenger.Register<AddedMessage>(this, OnProducerAdded);
    }

    private void OnProducerAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Producer")
            _producers.Add((ProducerModel)message.Value.What!);
        
    }

    public IEnumerable<ProducerModel> Producers => _producers;
    public IEnumerable<CatalogueModel> Catalogues => _catalogueModels;

    private void GetParts(int? id)
    {
        NameOfPart = _dataStore.CatalogueModels.Where(x => x.UniId == id).OrderBy(x => x.UniId).First().Name;
        IsDirty = false;
        var model = _dataStore.CatalogueModels.Where(x => x.UniId == id).OrderBy(x => x.UniId).First().Children?.Select(
            x => new CatalogueModel
            {
                MainCatId = x.MainCatId,
                ProducerId = x.ProducerId,
                ProducerName = x.ProducerName,
                UniValue = x.UniValue,
                UniId = x.UniId,
                Name = x.Name
            });
        if (model != null)
            _catalogueModels.AddRange(model);
    }
    [RelayCommand]
    private async Task FilterProducers(string value)
    {
        await foreach (var item in DataFiltering.FilterProducer(_dataStore.ProducerModels, value))
            _producers.Add(item);
    }

    partial void OnProducerSearchChanged(string value)
    {
        _producers.Clear();
        FilterProducersCommand.Execute(value);
    }
    partial void OnNameOfPartChanged(string value)
    {
        IsDirty = true;
    }

    [RelayCommand]
    private void AddNewPart()
    {
        _catalogueModels.Add(new CatalogueModel
        {
            MainCatId = null, UniId = _uniId, ProducerId = 1, ProducerName = "Неизвестный", Name = "Название не указано"
        });
        IsDirty = true;
    }

    private bool canDelete()
    {
        return _catalogueModels.Any();
    }

    [RelayCommand(CanExecute = nameof(canDelete))]
    private void RemovePart()
    {
        if (SelectedCatalogue != null)
        {
            if (SelectedCatalogue.MainCatId != null) ids.Add(SelectedCatalogue.MainCatId ?? 0);
            _catalogueModels.Remove(SelectedCatalogue);
            IsDirty = true;
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        _catalogueModels.Remove(_catalogueModels.Where(x => string.IsNullOrEmpty(x.UniValue)).ToList());

        var model = new CatalogueModel
        {
            UniId = _uniId,
            Name = NameOfPart,
            Children = new ObservableCollection<CatalogueModel>(_catalogueModels)
        };


        await _topModel.EditCatalogueAsync(model, ids);
        var what = await _topModel.GetCatalogueByIdAsync(_uniId ?? 5923);
        Messenger.Send(new EditedMessage(new ChangedItem { Where = "PartCatalogue", Id = _uniId, What = what }));
        _catalogueModels.Clear();
    }

    [RelayCommand]
    private async Task DeleteGroup()
    {
        await _topModel.DeleteGroupFromCatalogue(_uniId);
        Messenger.Send(new DeletedMessage(new DeletedItem { Where = "PartCatalogue", Id = _uniId }));
        _catalogueModels.Clear();
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
    [RelayCommand]
    private async Task AddToCatalogue()
    {
        var newId = await _topModel.AddNewCatalogue(new CatalogueModel
        {
            Name = NameOfPart,
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

    private void ChangeProducer(int index, int id, string name)
    {
        if (SelectedCatalogue != null && SelectedProducer != null)
        {
            SelectedCatalogue = null;
            SelectedProducer = null;
            _catalogueModels[index].ProducerId = id;
            _catalogueModels[index].ProducerName = name;
            IsDirty = true;
        }
    }
}