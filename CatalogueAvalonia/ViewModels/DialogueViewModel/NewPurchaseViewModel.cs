using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class NewPurchaseViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueServices;
    private readonly TopModel _topModel;
    private  ObservableCollection<ZakupkaAltModel> _zakupka;

    [ObservableProperty] private bool _canEditUsd;

    [ObservableProperty] private string _comment = string.Empty;

    [ObservableProperty] private bool _convertToUsd = true;

    [ObservableProperty] private bool _isVisibleConverter = true;

    [ObservableProperty] private DateTime _purchaseDate;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private ZakupkaAltModel? _selectedZakupka;

    [ObservableProperty] private decimal _totalSum;

    [ObservableProperty] private decimal _toUsd;

    public NewPurchaseViewModel()
    {
        _purchaseDate = DateTime.Now.Date;
        _currencies = new ObservableCollection<CurrencyModel>();
        _agents = new ObservableCollection<AgentModel>();
        _zakupka = new ObservableCollection<ZakupkaAltModel>();
        _canEditUsd = !ConvertToUsd;
    }

    public NewPurchaseViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueServices = dialogueService;

        _purchaseDate = DateTime.Now.Date;
        _canEditUsd = !ConvertToUsd;
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.IsZak == 1 && x.Id != 1));
        _zakupka = new ObservableCollection<ZakupkaAltModel>();
        Messenger.Register<AddedMessage>(this, OnItemAdded);
    }
    public NewPurchaseViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService, IEnumerable<ZakupkaAltModel> tempZak) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueServices = dialogueService;

        _purchaseDate = DateTime.Now.Date;
        _canEditUsd = !ConvertToUsd;
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.IsZak == 1 && x.Id != 1));
        _zakupka = new ObservableCollection<ZakupkaAltModel>();
        Messenger.Register<AddedMessage>(this, OnItemAdded);
        StartAddingCommand.Execute(tempZak);
    }

    private ZakupkaAltModel? _currAltModel;
    [RelayCommand]
    private async Task StartAdding(IEnumerable<ZakupkaAltModel> zakupki)
    {
        var mainWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
        foreach (var item in zakupki)
        {
            _currAltModel = item;
            await _dialogueServices.OpenDialogue(new CatalogueItemWindow(),
                new CatalogueItemViewModel(Messenger, _dataStore, _topModel, _dialogueServices, item), mainWindow);
            TotalSum = _zakupka.Sum(x => x.PriceSum);
        }
    }
    

    public IEnumerable<CurrencyModel> Currencies => _currencies;
    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<ZakupkaAltModel> Zakupka => _zakupka;

    private void OnItemAdded(object recipient, AddedMessage message)
    {
        var where = message.Value.Where;
        if (where == "CataloguePartItemSelected")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
                    _zakupka.Add(new ZakupkaAltModel
                    {
                        MainCatId = what.MainCatId,
                        MainCatName = what.Name,
                        MainName = mainName,
                        UniValue = what.UniValue,
                        Price = 0,
                    });
        }
        else if (where == "ZakupkaPartItemEdited")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
                if (SelectedZakupka != null)
                {
                    SelectedZakupka.MainCatId = what.MainCatId;
                    SelectedZakupka.MainCatName = what.Name;
                    SelectedZakupka.MainName = mainName;
                    SelectedZakupka.UniValue = what.UniValue;
                }
        }
        else if (where == "AutomatedZakupka")
        {
            var what = (CatalogueModel?)message.Value.What;
            var mainName = message.Value.MainName;
            if (what != null)
                _zakupka.Add(new ZakupkaAltModel
                {
                    MainCatId = what.MainCatId,
                    MainCatName = what.Name,
                    MainName = mainName,
                    UniValue = what.UniValue,
                    Price = _currAltModel!.Price,
                    Count = _currAltModel!.Count,
                    TextDecimal = _currAltModel!.Price.ToString(),
                    TextCount = _currAltModel!.TextCount
                });
        }
    }

    partial void OnSelectedCurrencyChanged(CurrencyModel? value)
    {
        if (value != null) ToUsd = value.ToUsd;
    }

    partial void OnToUsdChanged(decimal value)
    {
        if (SelectedCurrency != null && SelectedCurrency.Id == 2) ToUsd = 1;
    }

    partial void OnConvertToUsdChanged(bool value)
    {
        CanEditUsd = !ConvertToUsd;
    }

    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        SelectedZakupka = null;
        await _dialogueServices.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 0, _topModel, _dialogueServices), parent);
    }

    [RelayCommand]
    private void RemovePart(Window parent)
    {
        if (SelectedZakupka != null) _zakupka.Remove(SelectedZakupka);
    }

    [RelayCommand]
    private async Task ChangePart(Window parent)
    {
        await _dialogueServices.OpenDialogue(new CatalogueItemWindow(),
            new CatalogueItemViewModel(Messenger, _dataStore, 1, _topModel, _dialogueServices), parent);
    }

    public void RemoveWhereZero(IEnumerable<ZakupkaAltModel> altModels)
    {
        _zakupka.RemoveMany(altModels);
    }
    public AgentTransactionModel TransactionModel = new();
    [RelayCommand]
    private async Task SaveZakupka(Window parent)
    {
        if (SelectedAgent != null && SelectedCurrency != null)
        {
            List<ZakupkaAltModel> zakupkas = new();
            int currencyId = 2;
            if (ConvertToUsd)
                zakupkas.AddRange(_zakupka.Select(x => new ZakupkaAltModel
                {
                    Count = x.Count,
                    MainCatId = x.MainCatId,
                    MainName = x.MainName,
                    PriceSum = x.PriceSum,
                    MainCatName = x.MainCatName,
                    UniValue = x.UniValue,
                    Price = x.Price / SelectedCurrency.ToUsd
                }));
            else
            {
                currencyId = SelectedCurrency.Id ?? 2;
                zakupkas.AddRange(_zakupka);
            }
            var transactionSum = -1 * TotalSum;
            var balance = await _topModel.GetAgentsBalance(SelectedAgent.Id, SelectedCurrency.Id ?? default);
            var agentModel = new AgentTransactionModel
            {
                AgentId = SelectedAgent.Id,
                CurrencyId = SelectedCurrency.Id ?? default,
                TransactionDatatime = PurchaseDate.Date.ToString("dd.MM.yyyy"),
                TransactionStatus = 2,
                TransactionSum = transactionSum,
                Balance = balance + transactionSum
            };
            await _dialogueServices.OpenDialogue(new AddNewPayment(),
                new AddNewPaymentViewModel(Messenger, _topModel, _dataStore, agentModel, SelectedAgent.Name, TransactionModel),
                parent);
            parent.Close();
            var zakMain = new ZakupkiModel
            {
                AgentId = SelectedAgent.Id,
                CurrencyId = SelectedCurrency.Id ?? default,
                Datetime = PurchaseDate.Date.ToString("dd.MM.yyyy"),
                TotalSum = TotalSum,
                Comment = Comment
            };
            var catas = await _topModel.AddNewZakupkaAsync(_zakupka, zakMain, agentModel, TransactionModel, zakupkas, currencyId);
            
            Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
            Messenger.Send(new ActionMessage("Update"));
            
        }
    }
}