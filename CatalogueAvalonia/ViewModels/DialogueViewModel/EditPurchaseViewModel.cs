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
using MsBox.Avalonia;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class EditPurchaseViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueServices;
    private readonly TopModel _topModel;
    private readonly ObservableCollection<ZakupkaAltModel> _zakupka;
    private readonly ZakupkiModel _zakupkaMainGroup;

    [ObservableProperty] private bool _canEditUsd;

    [ObservableProperty] private string _comment = string.Empty;

    [ObservableProperty] private bool _convertToUsd = true;

    private readonly List<Tuple<int, int>> _deletedIds = new();

    [ObservableProperty] private bool _isVisibleConverter;

    private readonly Dictionary<int, int> _prevCounts = new();

    [ObservableProperty] private DateTime _purchaseDate;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private ZakupkaAltModel? _selectedZakupka;

    [ObservableProperty] private decimal _totalSum;

    [ObservableProperty] private decimal _toUsd;

    public bool IsDirty;

    public EditPurchaseViewModel()
    {
        _purchaseDate = DateTime.Now.Date;
        _currencies = new ObservableCollection<CurrencyModel>();
        _agents = new ObservableCollection<AgentModel>();
        _zakupka = new ObservableCollection<ZakupkaAltModel>();
        _canEditUsd = !ConvertToUsd;
    }

    public EditPurchaseViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService, ZakupkiModel zakupkaMainGroup) : base(messenger)
    {
        _zakupkaMainGroup = zakupkaMainGroup;
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueServices = dialogueService;
        _comment = (zakupkaMainGroup.Comment ?? string.Empty).TrimStart(' ').TrimEnd(' ');
        DateTime.TryParse(zakupkaMainGroup.Datetime, out _purchaseDate);
        _canEditUsd = !ConvertToUsd;
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.IsZak == 1 && x.Id != 1));
        _zakupka = new ObservableCollection<ZakupkaAltModel>();
        Messenger.Register<AddedMessage>(this, OnItemAdded);

        LoadZakupkiCommand.Execute(null);
    }

    public IEnumerable<CurrencyModel> Currencies => _currencies;
    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<ZakupkaAltModel> Zakupka => _zakupka;

    [RelayCommand]
    private async Task LoadZakupki()
    {
        var beenIds = new Dictionary<int, int>();
        _zakupka.AddRange(await _topModel.GetZakAltGroup(_zakupkaMainGroup.Id));

        foreach (var item in _zakupka)
        {
            var diff = await _topModel.CanDeleteProdaja(item.MainCatId ?? 1);
            
            
            if (diff != null)
            {
                if (!beenIds.ContainsKey(item.MainCatId ?? 0))
                    beenIds.Add(item.MainCatId ?? 0, diff ?? 0);
                var minCount = item.Count - beenIds[item.MainCatId ?? 0];
                beenIds[item.MainCatId ?? 0] -= item.Count ?? 0;
                if (minCount <= 0)
                {
                    item.MinCount = 0;
                    item.CanDelete = true;
                }
                else
                {
                    item.MinCount = minCount ?? 0;
                    item.CanDelete = false;
                }
            }

            if (!_prevCounts.ContainsKey(item.MainCatId ?? 1))
                _prevCounts.Add(item.MainCatId ?? 1, item.Count ?? 0);
            else
                _prevCounts[item.MainCatId ?? 1] += item.Count ?? 0;
        }


        if (_currencies.Any(x => x.Id == _zakupkaMainGroup.CurrencyId))
            SelectedCurrency = _currencies.First(x => x.Id == _zakupkaMainGroup.CurrencyId);
        if (_agents.Any(x => x.Id == _zakupkaMainGroup.AgentId))
            SelectedAgent = _agents.First(x => x.Id == _zakupkaMainGroup.AgentId);
        TotalSum = _zakupka.Sum(x => x.PriceSum);

        IsDirty = false;
    }

    partial void OnCommentChanged(string value)
    {
        IsDirty = true;
    }

    private void OnItemAdded(object recipient, AddedMessage message)
    {
        var where = message.Value.Where;
        if (where == "CataloguePartItemSelected")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
            {
                _zakupka.Add(new ZakupkaAltModel
                {
                    Id = null,
                    MainCatId = what.MainCatId,
                    MainCatName = what.Name,
                    MainName = mainName,
                    UniValue = what.UniValue,
                    ZakupkaId = _zakupkaMainGroup.Id
                });
                IsDirty = true;
            }
        }
        else if (where == "ZakupkaPartItemEdited")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
            {
                if (SelectedZakupka != null)
                {
                    if (SelectedZakupka.Id != null && !_deletedIds.Any(x => x.Item1 == SelectedZakupka.Id && x.Item2 == SelectedZakupka.MainCatId))
                        _deletedIds.Add(new Tuple<int, int>(SelectedZakupka.Id ?? 0, SelectedZakupka.MainCatId??0));
                    SelectedZakupka.Id = null;
                    SelectedZakupka.MainCatId = what.MainCatId;
                    SelectedZakupka.MainCatName = what.Name;
                    SelectedZakupka.MainName = mainName;
                    SelectedZakupka.UniValue = what.UniValue;
                }

                IsDirty = true;
            }
        }
    }

    partial void OnPurchaseDateChanged(DateTime value)
    {
        IsDirty = true;
    }

    partial void OnSelectedCurrencyChanged(CurrencyModel? value)
    {
        if (value != null)
        {
            ToUsd = value.ToUsd;
            IsDirty = true;
        }
    }

    partial void OnToUsdChanged(decimal value)
    {
        if (SelectedCurrency != null && SelectedCurrency.Id == 2) ToUsd = 1;
    }

    partial void OnConvertToUsdChanged(bool value)
    {
        CanEditUsd = !ConvertToUsd;
        IsDirty = true;
    }

    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        SelectedZakupka = null;
        await _dialogueServices.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 0, _topModel, _dialogueServices), parent);
    }

    [RelayCommand]
    private async Task RemovePart(Window parent)
    {
        if (SelectedZakupka != null)
        {
            if (SelectedZakupka.CanDelete)
            {
                if (SelectedZakupka.Id != null) _deletedIds.Add(new Tuple<int, int>(SelectedZakupka.Id ?? 0, SelectedZakupka.MainCatId??0));
                _zakupka.Remove(SelectedZakupka);
                IsDirty = true;
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("?",
                    "Данную запчасть нельзя поменять.").ShowWindowDialogAsync(parent);
            }
        }

        TotalSum = _zakupka.Sum(x => x.PriceSum);
    }

    [RelayCommand]
    private async Task ChangePart(Window parent)
    {
        if (SelectedZakupka != null)
        {
            if (SelectedZakupka.CanDelete)
                await _dialogueServices.OpenDialogue(new CatalogueItemWindow(),
                    new CatalogueItemViewModel(Messenger, _dataStore, 1, _topModel, _dialogueServices), parent);
            else
                await MessageBoxManager.GetMessageBoxStandard("?",
                    "Данную запчасть нельзя поменять.").ShowWindowDialogAsync(parent);
        }
    }

    public void RemoveWhereZero(IEnumerable<ZakupkaAltModel> altModels)
    {
        foreach (var item in altModels)
            if (item.Id != null)
                _deletedIds.Add(new Tuple<int, int>(item.Id ?? 0, item.MainCatId??0));
        _zakupka.RemoveMany(altModels);
    }

    [RelayCommand]
    private async Task SaveZakupka()
    {
        if (SelectedCurrency != null)
        {
            var catas = await _topModel.EditZakupkaAsync(_deletedIds, Zakupka, _prevCounts, SelectedCurrency, TotalSum,
                Converters.ToDateTimeSqlite(PurchaseDate.Date.ToString("dd.MM.yyyy")), _zakupkaMainGroup.TransactionId,
                Comment);
            Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
            Messenger.Send(new ActionMessage("Update"));
        }
    }

    [RelayCommand]
    private async Task DeleteAll()
    {
        var catas = await _topModel.DeleteZakupkaWithPricesReCount(_zakupkaMainGroup.TransactionId,
            _prevCounts.Select(x => new ZakupkaAltModel { MainCatId = x.Key, Count = x.Value }));

        Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
        Messenger.Send(new ActionMessage("Update"));
    }
}