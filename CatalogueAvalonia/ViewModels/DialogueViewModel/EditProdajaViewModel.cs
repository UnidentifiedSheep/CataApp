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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class EditProdajaViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly ObservableCollection<ProdajaAltModel> _prodajaAlts;
    private readonly ProdajaModel _prodajaModel;
    private readonly TopModel _topModel;

    [ObservableProperty] private string _comment = string.Empty;

    private readonly List<Tuple<int, decimal>> _deletedIds = new();

    [ObservableProperty] private bool _isVisibleConverter = true;

    [ObservableProperty] private int _overPrice = 20;

    private readonly Dictionary<int, int> _prevCounts = new();

    [ObservableProperty] private DateTime _prodajaDate;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private ProdajaAltModel? _selectedProdaja;

    [ObservableProperty] private decimal _totalSum;
    [ObservableProperty] private bool _isCommentVis = false;
    public bool IsEditingRestricted => true;

    public bool IsDirty;

    public EditProdajaViewModel()
    {
        _currencies = new ObservableCollection<CurrencyModel>();
        _agents = new ObservableCollection<AgentModel>();
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();
        _prodajaDate = DateTime.Now.Date;
    }

    public EditProdajaViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService, ProdajaModel prodMainGroup) : base(messenger)
    {
        _prodajaModel = prodMainGroup;
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        _comment = (prodMainGroup.Comment ?? string.Empty).TrimStart(' ').TrimEnd(' ');
        DateTime.TryParse(prodMainGroup.Datetime, out _prodajaDate);
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.Id != 1));
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();
        Messenger.Register<AddedMessage>(this, OnItemAdded);

        LoadProdajaCommand.Execute(null);
    }

    public IEnumerable<CurrencyModel> Currencies => _currencies;
    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<ProdajaAltModel> Prodaja => _prodajaAlts;

    [RelayCommand]
    public async Task LoadProdaja()
    {
        _prodajaAlts.AddRange(await _topModel.GetProdajaAltGroupAsync(_prodajaModel.Id));

        foreach (var item in _prodajaAlts)
        {
            var mainCatPrices = await _topModel.GetMainCatPricesByIdAsync(item.MainCatId ?? 5923);
            item.MaxCount = (item.Count ?? 0) + mainCatPrices.Sum(x => x.Count ?? 0);
            if (!_prevCounts.ContainsKey(item.MainCatId ?? 0))
                _prevCounts.Add(item.MainCatId ?? 0, item.Count ?? 0);
            else
                _prevCounts[item.MainCatId ?? 0] += item.Count ?? 0;
        }

        if (_currencies.Any(x => x.Id == _prodajaModel.CurrencyId))
            SelectedCurrency = _currencies.First(x => x.Id == _prodajaModel.CurrencyId);
        if (_agents.Any(x => x.Id == _prodajaModel.AgentId))
            SelectedAgent = _agents.First(x => x.Id == _prodajaModel.AgentId);
        TotalSum = _prodajaAlts.Sum(x => x.PriceSum);

        IsDirty = false;
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
                if (!_prodajaAlts.Any(x => x.MainCatId == what.MainCatId))
                    if (what.Children != null && what.Children.Any())
                    {
                        decimal price = 0;
                        var firstPrice = what.Children.First();
                        var partsCurr = _dataStore.CurrencyModels.FirstOrDefault(x => x.Id == firstPrice.CurrencyId);
                        var overPr = (100 + OverPrice) / 100.0m;

                        if (partsCurr != null && SelectedCurrency != null)
                            price = Math.Round((firstPrice.Price / partsCurr.ToUsd * SelectedCurrency.ToUsd * overPr) ?? 0, 2);

                        _prodajaAlts.Add(new ProdajaAltModel
                        {
                            MainCatId = what.MainCatId,
                            MainCatName = what.Name,
                            MainName = mainName,
                            MaxCount = what.Count,
                            UniValue = what.UniValue,
                            Price = price,
                            CurrencyInitialId = firstPrice.CurrencyId,
                            InitialPrice = firstPrice.Price ?? 0
                        });
                    }

                IsDirty = true;
            }
        }
        else if (where == "ZakupkaPartItemEdited")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
            {
                if (SelectedProdaja != null)
                {
                    if (SelectedProdaja.Id != null && !_deletedIds.Any(x => x.Item1 == SelectedProdaja.Id))
                        _deletedIds.Add(new Tuple<int, decimal>(SelectedProdaja.Id ?? 0, SelectedProdaja.Price ?? 0));
                    SelectedProdaja.Id = null;
                    SelectedProdaja.MainCatId = what.MainCatId;
                    SelectedProdaja.MainCatName = what.Name;
                    SelectedProdaja.MainName = mainName;
                    SelectedProdaja.UniValue = what.UniValue;
                }

                IsDirty = true;
            }
        }
    }

    partial void OnProdajaDateChanged(DateTime value)
    {
        IsDirty = true;
    }

    partial void OnSelectedCurrencyChanged(CurrencyModel? value)
    {
        if (value != null) IsDirty = true;
    }

    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        SelectedProdaja = null;
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 0, _topModel, _dialogueService), parent);
    }

    [RelayCommand]
    private void DeletePart()
    {
        if (SelectedProdaja != null)
        {
            if (SelectedProdaja.Id != null)
                _deletedIds.Add(new Tuple<int, decimal>(SelectedProdaja.Id ?? 0, SelectedProdaja.Price ?? 0));
            _prodajaAlts.Remove(SelectedProdaja);
            IsDirty = true;
        }

        TotalSum = _prodajaAlts.Sum(x => x.PriceSum);
    }

    [RelayCommand]
    private async Task ChangePart(Window parent)
    {
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 1, _topModel, _dialogueService), parent);
    }

    partial void OnCommentChanged(string value)
    {
        Comment = value.Replace("/", "");
        IsDirty = true;
    }

    public void RemoveWhereZero(IEnumerable<ProdajaAltModel> altModels)
    {
        _prodajaAlts.RemoveMany(altModels);
    }
    [RelayCommand]
    private void MakeCommentVis()
    {
        IsCommentVis = !IsCommentVis;
    }

    [RelayCommand]
    private async Task SaveZakupka()
    {
        if (SelectedCurrency != null)
        {
            var catas = await _topModel.EditProdajaAsync(_deletedIds, Prodaja, _prevCounts, SelectedCurrency,
                Converters.ToDateTimeSqlite(ProdajaDate.Date.ToString("dd.MM.yyyy")), TotalSum,
                _prodajaModel.TransactionId, $" {Comment} ");
            var balances = await _topModel.GetAgentsBalance(SelectedAgent!.Id);
            Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
            Messenger.Send(new ActionMessage(new ActionM("Update", balances)));
        }
    }

    [RelayCommand]
    private async Task DeleteAll()
    {
        IEnumerable<CatalogueModel> catas = new List<CatalogueModel>();

        catas = await _topModel.DeleteProdajaAsync(_prodajaModel.TransactionId, _prodajaAlts, SelectedCurrency.Id ?? 0);
        var balances = await _topModel.GetAgentsBalance(SelectedAgent!.Id);
        Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
        Messenger.Send(new ActionMessage(new ActionM("Update", balances)));
    }
}