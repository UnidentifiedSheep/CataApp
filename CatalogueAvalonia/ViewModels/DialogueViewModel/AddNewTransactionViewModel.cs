using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewTransactionViewModel : ViewModelBase
{
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly DataStore _dataStore;
    private readonly TopModel _topModel;

    [ObservableProperty] private string _actionName = "Транзакция:";

    private readonly int _agentId;

    [ObservableProperty] private bool _convertFromCurr;

    [ObservableProperty] private DateTime _date;

    [ObservableProperty] private bool _isEnb = true;

    [ObservableProperty] private bool _isVisAndEnb;

    [ObservableProperty] private decimal _minTrSum = decimal.MinValue;

    [ObservableProperty] private string _nameOfAgent = string.Empty;

    [ObservableProperty] private CurrencyModel? _selectedConvertCurrency;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private decimal? _transactionSum;
    [ObservableProperty] private string? _transactionText = "0";

    public AddNewTransactionViewModel()
    {
        _currencies = new ObservableCollection<CurrencyModel>();
        _isVisAndEnb = true;
        Date = DateTime.Now.Date;
    }
    

    /// <summary>
    ///     Для создания транзакции.
    /// </summary>
    /// <param name="messenger"></param>
    /// <param name="topModel"></param>
    /// <param name="dataStore"></param>
    /// <param name="nameOfAgent"></param>
    /// <param name="agentId"></param>
    /// <param name="action">2 - Новый расход, 3 - Новый приход</param>
    public AddNewTransactionViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore, string nameOfAgent,
        int agentId, int action) : base(messenger)
    {
        Action = action;
        _isVisAndEnb = false;
        _topModel = topModel;
        _dataStore = dataStore;
        _nameOfAgent = nameOfAgent;
        _agentId = agentId;
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        SelectedCurrency = _currencies.FirstOrDefault(x => x.CurrencyName.ToLower().Contains("рубл"));
        Date = DateTime.Now.Date;

        OnStart();
    }

    public AgentTransactionModel? TransactionData { get; }

    public int Action { get; }

    public IEnumerable<CurrencyModel> Currencies => _currencies;

    private void OnStart()
    {
        if (Action == 2)
        {
            MinTrSum = 0;
            ActionName = "Приход:";
        }
        else if (Action == 3)
        {
            MinTrSum = 0;
            ActionName = "Расход:";
        }
    }

    partial void OnTransactionSumChanged(decimal? value)
    {
        if (value == null)
            TransactionSum = 0m;
    }

    partial void OnTransactionTextChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
            value = "0,00";
        TransactionText = value.Replace('.', ',');
        
    }

    partial void OnConvertFromCurrChanged(bool value)
    {
        if (value)
            IsVisAndEnb = true;
        else
            IsVisAndEnb = false;
    }

    [RelayCommand]
    private async Task AddTransaction()
    {
        if (Action == 0)
        {
            await AddNewTransactionNormal(TransactionSum ?? 0);
        }
        else if (Action == 2)
        {
            await AddNewTransactionNormal(-1 * (TransactionSum ?? 0));
        }
        else if (Action == 3)
        {
            await AddNewTransactionNormal(TransactionSum ?? 0);
        }
        else if (Action == 1)
        {
            if (ConvertFromCurr)
            {
                if (SelectedCurrency != null && SelectedConvertCurrency != null)
                {
                    var status = 0;
                    if (TransactionSum > 0)
                        status = 1;
                    else if (TransactionSum < 0)
                        status = 0;

                    var balance = await _topModel.GetAgentsBalance(_agentId, SelectedCurrency.Id ?? default);
                    decimal sum = -1 * (TransactionSum ?? 0) / SelectedConvertCurrency.ToUsd * SelectedCurrency.ToUsd;
                    var id = await _topModel.AddNewTransactionAsync(new AgentTransactionModel
                    {
                        AgentId = _agentId,
                        CurrencyId = SelectedCurrency.Id ?? default,
                        TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
                        TransactionStatus = status,
                        TransactionSum = sum,
                        Balance = balance + sum
                    });
                    var balances = await _topModel.GetAgentsBalance(_agentId);
                    Messenger.Send(new ActionMessage(new ActionM("Update", balances)));
                }
            }
            else
            {
                await AddNewTransactionNormal(TransactionSum ?? 0);
            }
        }
    }

    [RelayCommand]
    private async Task AddNewTransactionNormal(decimal transactionSum)
    {
        if (SelectedCurrency != null)
        {
            var status = 0;
            if (transactionSum > 0)
                status = 1;
            else if (transactionSum < 0)
                status = 0;

            var balance = await _topModel.GetAgentsBalance(_agentId, SelectedCurrency.Id ?? 1);
            var id = await _topModel.AddNewTransactionAsync(new AgentTransactionModel
            {
                AgentId = _agentId,
                CurrencyId = SelectedCurrency.Id ?? default,
                TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
                TransactionStatus = status,
                TransactionSum = transactionSum,
                Balance = balance + transactionSum
            });
            var balances = await _topModel.GetAgentsBalance(_agentId);
            Messenger.Send(new ActionMessage(new ActionM("Update", balances)));
        }
    }
}