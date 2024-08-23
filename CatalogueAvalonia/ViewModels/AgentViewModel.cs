using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.ViewModels;

public partial class AgentViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<AgentTransactionModel> _agentTransactions;
    private readonly ObservableCollection<CurrencyModel> _currencyModels;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly TopModel _topModel;

    [ObservableProperty] private string _agentSearchField = string.Empty;

    [ObservableProperty] private DateTime _startDate;
    [ObservableProperty] private DateTime _endDate;

    [ObservableProperty] private bool _isCurrencyVisible = true;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddNewAgentCommand))]
    private bool _isDataBaseLoaded;

    [ObservableProperty] private bool _isDebtCreditVisible;

    [ObservableProperty] private bool _isLoaded = !false;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private CurrencyModel? _selectedCurrency;

    [ObservableProperty] private AgentTransactionModel? _selectedTransaction;


    [ObservableProperty] private decimal _totalCredit;

    [ObservableProperty] private decimal _totalDebt;
    [ObservableProperty] private bool _isOverPriceVis = false;

    public AgentViewModel()
    {
        _agents = new ObservableCollection<AgentModel>();
        _currencyModels = new ObservableCollection<CurrencyModel>();
        _agentTransactions = new ObservableCollection<AgentTransactionModel>();
        _isDebtCreditVisible = true;

        _startDate = DateTime.Now.AddMonths(-1).Date;
        _endDate = DateTime.Now.Date;
    }

    public AgentViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        _agents = new ObservableCollection<AgentModel>();
        _currencyModels = new ObservableCollection<CurrencyModel>();
        _agentTransactions = new ObservableCollection<AgentTransactionModel>();

        _startDate = DateTime.Now.AddMonths(-1).Date;
        _endDate = DateTime.Now.Date;
        
        Messenger.Register<ActionMessage>(this, OnAction);
        Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
        Messenger.Register<EditedMessage>(this, OnDataBaseEdited);
    }

    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<CurrencyModel> Currencies => _currencyModels;
    public IEnumerable<AgentTransactionModel> AgentTransactions => _agentTransactions;

    private void OnDataBaseEdited(object recipient, EditedMessage message)
    {
        var where = message.Value.Where;
        if (where == "Currencies")
        {
            var what = message.Value.What as IEnumerable<CurrencyModel>;
            if (what != null)
                Dispatcher.UIThread.Post(() =>
                {
                    _currencyModels.Clear();
                    _currencyModels.AddRange(what);
                });
        }
    }

    [RelayCommand]
    private void SetVisability()
    {
        IsOverPriceVis = !IsOverPriceVis;
    }

    private void OnAction(object recipient, ActionMessage message)
    {
        if (message.Value == "DataBaseLoaded")
            Dispatcher.UIThread.Post(() =>
            {
                IsLoaded = !true;
                IsDataBaseLoaded = true;
                _agents.AddRange(_dataStore.AgentModels.Where(x => x.Id != 1));
                _currencyModels.AddRange(_dataStore.CurrencyModels);
            });
        else if (message.Value == "Update")
            Dispatcher.UIThread.Post(() => GetTransactionsCommand.Execute(null));
    }

    private void OnDataBaseAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Agent")
        {
            var what = (AgentModel?)message.Value.What;
            if (what != null)
                Dispatcher.UIThread.Post(() => _agents.Add(what));
        }
    }

    [RelayCommand]
    private async Task FilterAgent(string value)
    {
        _agents.Clear();
        await foreach (var agent in DataFiltering.FilterAgents(_dataStore.AgentModels.Where(x => x.Id != 1), value))
            _agents.Add(agent);
    }

    partial void OnAgentSearchFieldChanged(string value)
    {
        if (value.Length >= 2)
        {
            FilterAgentCommand.Execute(value);
        }
        else
        {
            if (_dataStore.AgentModels.Count != _agents.Count)
            {
                _agents.Clear();
                _agents.AddRange(_dataStore.AgentModels.Where(x => x.Id != 1));
            }
        }
    }

    partial void OnSelectedCurrencyChanged(CurrencyModel? value)
    {
        if (SelectedCurrency != null && SelectedCurrency.Id == 1)
        {
            IsCurrencyVisible = true;
            IsDebtCreditVisible = false;
        }
        else
        {
            IsCurrencyVisible = false;
            IsDebtCreditVisible = true;
        }

        GetTransactionsCommand.Execute(null);
    }

    partial void OnSelectedAgentChanged(AgentModel? value)
    {
        GetTransactionsCommand.Execute(null);
    }

    partial void OnEndDateChanged(DateTime value)
    {
        
        if (value >= StartDate)
            GetTransactionsCommand.Execute(null);
        else
            MessageBoxManager
                .GetMessageBoxStandard("Неверные даты", "Начальная дата должна быть меньше или равна дате конца.")
                .ShowWindowDialogAsync((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!);
            
        
    }

    
    partial void OnStartDateChanged(DateTime value)
    {
        if (EndDate >= value)
            GetTransactionsCommand.Execute(null);
        else
            MessageBoxManager
                .GetMessageBoxStandard("Неверные даты", "Начальная дата должна быть меньше или равна дате конца.")
                .ShowWindowDialogAsync((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!);
        
    }

    [RelayCommand]
    private async Task GetTransactions()
    {
        TotalDebt = 0;
        TotalCredit = 0;
        _agentTransactions.Clear();
        if (SelectedAgent != null && SelectedCurrency != null)
        {
            var model = await _topModel.GetAgentTransactionsByIdsAsync(SelectedAgent.Id, SelectedCurrency.Id ?? default,
                Converters.ToDateTimeSqlite(StartDate.ToString("dd.MM.yyyy")),
                Converters.ToDateTimeSqlite(EndDate.ToString("dd.MM.yyyy")));
            var transactions = model.Where(x => x.TransactionStatus != 3).OrderByDescending(x => x.ResultDate).ToList();
            _agentTransactions.AddRange(transactions);

            if (SelectedCurrency.Id == 1)
            {
                IsCurrencyVisible = false;
                IsCurrencyVisible = true;
            }

            var balance = await _topModel.GetAgentsBalance(SelectedAgent.Id, SelectedCurrency.Id ?? default);
            if (balance > 0)
                TotalDebt = balance;
            else if (balance < 0)
                TotalCredit = balance * -1;
        }
    }

    [RelayCommand]
    private async Task EditAgent()
    {
        if (SelectedAgent != null)
        {
            await _topModel.EditAgentAsync(SelectedAgent);
            Messenger.Send(new EditedMessage(new ChangedItem { Id = SelectedAgent.Id, Where = "AgentEdited" }));
        }
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task AddNewAgent(Window parent)
    {
        await _dialogueService.OpenDialogue(new AddNewAgentWindow(), new AddNewAgentViewModel(Messenger, _topModel, _dataStore),
            parent);
    }

    private bool canDoWithAgent()
    {
        return SelectedAgent != null;
    }

    [RelayCommand(CanExecute = nameof(canDoWithAgent))]
    private async Task DeleteAgent()
    {
        if (SelectedAgent != null)
        {
            await _topModel.DeleteAgentAsync(SelectedAgent.Id);
            Messenger.Send(new DeletedMessage(new DeletedItem { Id = SelectedAgent.Id, Where = "Agent" }));
            _agents.Remove(SelectedAgent);
        }
    }

    [RelayCommand(CanExecute = nameof(canDoWithAgent))]
    private async Task AddNewTransaction(Window parent)
    {
        if (SelectedAgent != null)
            await _dialogueService.OpenDialogue(new AddNewTransactionWindow(),
                new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id,
                    0), parent);
    }

    [RelayCommand(CanExecute = nameof(canDoWithAgent))]
    private async Task AddNewConsumption(Window parent)
    {
        if (SelectedAgent != null)
            await _dialogueService.OpenDialogue(new AddNewTransactionWindow(),
                new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id,
                    2), parent);
    }

    [RelayCommand(CanExecute = nameof(canDoWithAgent))]
    private async Task AddNewInCome(Window parent)
    {
        if (SelectedAgent != null)
            await _dialogueService.OpenDialogue(new AddNewTransactionWindow(),
                new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id,
                    3), parent);
    }

    private bool canDoWithTransaction()
    {
        return SelectedTransaction != null;
    }

    [RelayCommand(CanExecute = nameof(canDoWithTransaction))]
    private async Task DeleteTransaction(Window parent)
    {
        if (SelectedAgent != null)
            if (SelectedCurrency != null && SelectedTransaction != null && SelectedTransaction.CurrencyId != 1)
            {
                if (SelectedTransaction.TransactionStatus == 2 || SelectedTransaction.TransactionStatus == 4)
                {
                    var res = await MessageBoxManager.GetMessageBoxStandard("Удалить закупку/продажу",
                            "Нельзя удалить транзакцию закупки/продажу.\n Для удаления транзакции нужно удалить закупку.")
                        .ShowWindowDialogAsync(parent);
                }
                else
                {
                    var res = await MessageBoxManager.GetMessageBoxStandard("Удалить транзакцию",
                        "Вы уверенны что хотите удалить транзакцию?",
                        ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
                    if (res == ButtonResult.Yes)
                    {
                        await _topModel.DeleteAgentTransactionAsync(SelectedAgent.Id, SelectedTransaction.CurrencyId,
                            SelectedTransaction.Id);
                        GetTransactionsCommand.Execute(null);
                    }
                }
            }
    }
}