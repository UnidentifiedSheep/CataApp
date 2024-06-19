using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
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

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class CatalogueItemViewModel : ViewModelBase
{
    private readonly ObservableCollection<CatalogueModel> _catalogueModels;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly TopModel _topModel;
    private readonly int? _actionNumber;

    private bool _isFilteringByUniValue;

    [ObservableProperty] private string _partName = string.Empty;

    [ObservableProperty] private string _partUniValue = string.Empty;

    private CatalogueModel? _selecteditem;

    public CatalogueItemViewModel()
    {
    }

    public CatalogueItemViewModel(IMessenger messenger, DataStore dataStore, int? actionNumber, TopModel topModel,
        IDialogueService dialogueService) : base(messenger)
    {
        _actionNumber = actionNumber;
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        _catalogueModels = new ObservableCollection<CatalogueModel>();

        CatalogueModels = new HierarchicalTreeDataGridSource<CatalogueModel>(_catalogueModels)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<CatalogueModel>(
                    new TextColumn<CatalogueModel, string>
                        ("Название", x => x.Name, new GridLength(580)), x => x.Children),
                new TextColumn<CatalogueModel, string>(
                    "Номер запчасти", x => x.UniValue, new GridLength(150)),
                new TextColumn<CatalogueModel, string>(
                    "Производитель", x => x.ProducerName, new GridLength(130)),
                new TextColumn<CatalogueModel, decimal?>(
                    "Цена", x => x.VisiblePrice, GridLength.Star),
                new TextColumn<CatalogueModel, int>(
                    "Количество", x => x.Count, GridLength.Star)
            }
        };
        _catalogueModels.AddRange(_dataStore.CatalogueModels);

        Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
    }

    public HierarchicalTreeDataGridSource<CatalogueModel> CatalogueModels { get; }

    private void OnDataBaseAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Catalogue")
        {
            var what = message.Value.What as CatalogueModel;
            if (what != null)
                Dispatcher.UIThread.Post(() =>
                {
                    _catalogueModels.Add(what);
                    CatalogueModels.RowSelection!.Deselect(CatalogueModels.RowSelection.SelectedIndex);
                });
        }
    }

    partial void OnPartNameChanged(string value)
    {
        var filter = new AsyncRelayCommand(async token =>
        {
            _catalogueModels.Clear();
            await foreach (var res in DataFiltering.FilterByMainName(_dataStore.CatalogueModels, value, token))
                _catalogueModels.Add(res);
        });
        if (_isFilteringByUniValue)
        {
        }
        else if (value.Length >= 3)
        {
            filter.Execute(null);
        }
        else if (value.Length <= 2 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
        {
            _catalogueModels.Clear();
            _catalogueModels.AddRange(_dataStore.CatalogueModels);
        }
    }

    partial void OnPartUniValueChanged(string value)
    {
        var filter = new AsyncRelayCommand(async token =>
        {
            _catalogueModels.Clear();
            await foreach (var res in DataFiltering.FilterByUniValue(_dataStore.CatalogueModels, value, token))
                _catalogueModels.Add(res);
        });
        if (value.Length >= 2)
        {
            filter.Execute(null);
            _isFilteringByUniValue = true;
        }
        else if (value.Length <= 1 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
        {
            _isFilteringByUniValue = false;
            _catalogueModels.Clear();
            _catalogueModels.AddRange(_dataStore.CatalogueModels);
            OnPartNameChanged(PartName);
        }
    }

    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        await _dialogueService.OpenDialogue(new AddNewPartView(),
            new AddNewPartViewModel(Messenger, _dataStore, _topModel), parent);
    }

    [RelayCommand]
    private async Task SelectPart(Window parent)
    {
        _selecteditem = CatalogueModels.RowSelection?.SelectedItem;
        if (_selecteditem != null && _selecteditem.MainCatId != null && _selecteditem.UniId != null)
        {
            var selectedPart = _selecteditem;
            var parentIndex = new IndexPath(CatalogueModels.RowSelection!.SelectedIndex.ToArray()[0]);
            CatalogueModels.RowSelection.SelectedIndex = parentIndex;
            var parentItem = CatalogueModels.RowSelection.SelectedItem;

            if (parentItem != null)
            {
                if (_actionNumber == 0 || _actionNumber == null)
                {
                    Messenger.Send(new AddedMessage(new ChangedItem
                    {
                        Id = _selecteditem.MainCatId, What = selectedPart, Where = "CataloguePartItemSelected",
                        MainName = parentItem.Name
                    }));
                    parent.Close();
                }
                else if (_actionNumber == 1)
                {
                    Messenger.Send(new AddedMessage(new ChangedItem
                    {
                        Id = _selecteditem.MainCatId, What = selectedPart, Where = "ZakupkaPartItemEdited",
                        MainName = parentItem.Name
                    }));
                    parent.Close();
                }
            }
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard("?",
                "Выбранная вами запчасть является либо 'Основной группой' либо 'Ценой'.").ShowWindowDialogAsync(parent);
        }
    }
}