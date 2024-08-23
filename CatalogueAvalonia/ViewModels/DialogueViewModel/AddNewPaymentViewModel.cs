using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewPaymentViewModel : ViewModelBase
{

    private readonly ObservableCollection<CurrencyModel> _currencies;
    public IEnumerable<CurrencyModel> Currencies => _currencies;
    private readonly DataStore _dataStore;
    private readonly TopModel _topModel;
    private AgentTransactionModel _agentTransaction;
    public AgentTransactionModel? TransactionData { get; private set; }
    
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
    
    public AddNewPaymentViewModel()
    {
        
    }

    public AddNewPaymentViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore,
        AgentTransactionModel transactionData, string nameOfAgent, AgentTransactionModel agentTransaction) : base(messenger)
    {
        TransactionData = transactionData;
        _isVisAndEnb = false;
        _isEnb = false;
        _topModel = topModel;
        _dataStore = dataStore;
        _nameOfAgent = nameOfAgent;
        _agentId = transactionData.AgentId;
        Date = DateTime.Now.Date;
        TransactionSum = transactionData.TransactionSum;
        _agentTransaction = agentTransaction;
        
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        SelectedCurrency = _currencies.FirstOrDefault(x => x.Id == transactionData.CurrencyId);
        OnStart();
    }

    private void OnStart()
    {
        if (TransactionSum < 0)
        {
            TransactionSum *= -1;
            TransactionText = (TransactionData!.TransactionSum * (-1)).ToString();
        }
        else
        {
            TransactionText = TransactionData!.TransactionSum.ToString();
        }
    }
    partial void OnConvertFromCurrChanged(bool value)
    {
        if (value)
            IsVisAndEnb = true;
        else
            IsVisAndEnb = false;
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
            _agentTransaction.AgentId = _agentId;
            _agentTransaction.CurrencyId = SelectedCurrency.Id ?? default;
            _agentTransaction.TransactionDatatime = Date.Date.ToString("dd.MM.yyyy");
            _agentTransaction.TransactionStatus = status;
            _agentTransaction.TransactionSum = transactionSum;
            /*int id = await _topModel.AddNewTransactionAsync(new AgentTransactionModel
            {
                AgentId = _agentId,
                CurrencyId = SelectedCurrency.Id ?? default,
                TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
                TransactionStatus = status,
                TransactionSum = transactionSum,
                Balance = balance + transactionSum
            });*/
        }
    }

    [RelayCommand]
    private async Task AddTransaction(bool payAll)
    {
        if (TransactionData!.TransactionSum < 0 && payAll)
            TransactionSum *= -1;
        else if (TransactionData!.TransactionSum > 0)
        {
            TransactionSum *= -1;
        }
        
        
        if (ConvertFromCurr)
        {
            if (SelectedCurrency != null && SelectedConvertCurrency != null)
            {
                decimal sum = (TransactionSum ?? 0) / SelectedConvertCurrency.ToUsd * SelectedCurrency.ToUsd; 
                await AddNewTransactionNormal(sum);
            }
        }
        else
        {
            await AddNewTransactionNormal(TransactionSum ?? 0);
        }
    }
}