using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

public partial class NewProdajaViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly ObservableCollection<ProdajaAltModel> _prodajaAlts;
    private readonly TopModel _topModel;

    [ObservableProperty] private string _comment = string.Empty;

    [ObservableProperty] private bool _isVisibleConverter = true;

    [ObservableProperty] private int? _overPrice = 0;

    [ObservableProperty] private DateTime _prodajaDate;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private ProdajaAltModel? _selectedProdaja;

    [ObservableProperty] private decimal _totalSum;

    [ObservableProperty] private string _producerSearch = string.Empty;
    [ObservableProperty] private bool _producerSearchOpen = false;
    public bool IsEditingRestricted => false;

    public NewProdajaViewModel()
    {
        _currencies = new ObservableCollection<CurrencyModel>();
        _agents = new ObservableCollection<AgentModel>();
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();
        _prodajaDate = DateTime.Now.Date;
    }

    [RelayCommand]
    private async Task FilterAgent(string value)
    {
        _agents.Clear();
        await foreach (var agent in DataFiltering.FilterAgents(_dataStore.AgentModels, value))
        {
            if (agent.Id != 1)
                _agents.Add(agent);
            
        }
    }

    partial void OnProducerSearchChanged(string value)
    {
        if (value.Length >= 1)
        {
            ProducerSearchOpen = true;
            FilterAgentCommand.Execute(value);
        }
        else
        {
            ProducerSearchOpen = false;
            if (_dataStore.AgentModels.Count != _agents.Count)
            {
                _agents.Clear();
                _agents.AddRange(_dataStore.AgentModels.Where(x => x.Id != 1));
            }
        }
    }
    partial void OnSelectedAgentChanged(AgentModel? value)
    {
        if (value == null) return;
        
        ProducerSearch = string.Empty;
        OverPrice = value.OverPrice;

    }

    public NewProdajaViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore,
        IDialogueService dialogueService) : base(messenger)
    {
        _topModel = topModel;
        _dataStore = dataStore;
        _dialogueService = dialogueService;

        _prodajaDate = DateTime.Now.Date;
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.Id != 1));
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();

        Messenger.Register<AddedMessage>(this, OnItemAdded);
        OnStart();
    }

    private void OnStart()
    {
        SelectedCurrency = _currencies.FirstOrDefault(x => x.CurrencyName.Contains("Рубл"));
    }

    public IEnumerable<CurrencyModel> Currencies => _currencies;
    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<ProdajaAltModel> Prodaja => _prodajaAlts;

    private void OnItemAdded(object recipient, AddedMessage message)
    {
        var where = message.Value.Where;
        if (where == "CataloguePartItemSelected")
        {
            decimal price = 0;
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
                if (!_prodajaAlts.Any(x => x.MainCatId == what.MainCatId))
                    if (what.Children != null && what.Children.Any())
                    {
                        var firstPrice = what.Children.OrderByDescending(x => x.Count).First();
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
                            Count = 0,
                            TextCont = "0",
                            UniValue = what.UniValue,
                            Price = price,
                            TextDecimal = Convert.ToString(price),
                            CurrencyInitialId = firstPrice.CurrencyId,
                            InitialPrice = firstPrice.Price ?? 0
                        });
                    }
        }
        else if (where == "ZakupkaPartItemEdited")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
                if (!_prodajaAlts.Any(x => x.MainCatId == what.MainCatId))
                    if (SelectedProdaja != null)
                    {
                        SelectedProdaja.MainCatId = what.MainCatId;
                        SelectedProdaja.MainCatName = what.Name;
                        SelectedProdaja.MainName = mainName;
                        SelectedProdaja.UniValue = what.UniValue;
                        SelectedProdaja.MaxCount = what.Count;
                        SelectedProdaja.Count = 0;
                        SelectedProdaja.TextCont = "0";
                    }
        }
    }
    partial void OnCommentChanged(string value)
    {
        Comment = value.Replace("/", "");
    }

    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        SelectedProdaja = null;
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 0, _topModel, _dialogueService), parent);
    }

    [RelayCommand]
    private async Task ChangePart(Window parent)
    {
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 1, _topModel, _dialogueService), parent);
    }

    [RelayCommand]
    private void DeletePart(Window parent)
    {
        if (SelectedProdaja != null) _prodajaAlts.Remove(SelectedProdaja);
        TotalSum = _prodajaAlts.Sum(x => x.PriceSum);
    }

    public void RemoveWhereZero(IEnumerable<ProdajaAltModel> models)
    {
        _prodajaAlts.RemoveMany(models);
    }
    [RelayCommand]
    private async Task LookForPrices(Window parent)
    {
        if (SelectedProdaja != null)
        {
            await _dialogueService.OpenDialogue(new EditPricesWindow(),
                new EditPricesViewModel(Messenger, _topModel, _dataStore, SelectedProdaja.MainCatId ?? 0,
                    SelectedProdaja.UniValue ?? ""), parent);
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard("Цены",
                $"Для начала выбирите запчасть").ShowWindowDialogAsync(parent);
        }
    }

    [RelayCommand]
    private async Task SaveChanges(Window parent)
    {
        if (SelectedAgent != null && SelectedCurrency != null)
        {
            var lastTransaction =
                await _topModel.GetLastTransactionAsync(SelectedAgent.Id, SelectedCurrency.Id ?? default);
            var agentModel = new AgentTransactionModel
            {
                AgentId = SelectedAgent.Id,
                CurrencyId = SelectedCurrency.Id ?? 1,
                TransactionDatatime = ProdajaDate.Date.ToString("dd.MM.yyyy"),
                TransactionSum = TotalSum,
                Balance = lastTransaction.Balance + TotalSum,
                TransactionStatus = 4
            };
            var transactionId = await _topModel.AddNewTransactionAsync(agentModel);
            await _dialogueService.OpenDialogue(new AddNewPayment(),
                new AddNewPaymentViewModel(Messenger, _topModel, _dataStore, agentModel, SelectedAgent.Name),
                parent);
            var mainModel = new ProdajaModel
            {
                AgentId = SelectedAgent.Id,
                Datetime = Converters.ToDateTimeSqlite(ProdajaDate.Date.ToString("dd.MM.yyyy")),
                Comment = $" {Comment} ",
                CurrencyId = SelectedCurrency.Id ?? 1,
                TotalSum = TotalSum,
                TransactionId = transactionId
            };
            parent.Close();
            var catas = await _topModel.AddNewProdaja(Prodaja, mainModel);
            Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
            Messenger.Send(new ActionMessage("Update"));
        }
    }
}