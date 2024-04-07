using Avalonia.Controls;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CatalogueAvalonia.ViewModels
{
	public partial class AgentViewModel : ViewModelBase
	{
		private readonly ObservableCollection<AgentModel> _agents;
		public IEnumerable<AgentModel> Agents => _agents;
		private readonly ObservableCollection<CurrencyModel> _currencyModels;
		public IEnumerable<CurrencyModel> Currencies => _currencyModels;
		private readonly ObservableCollection<AgentTransactionModel> _agentTransactions;
		public IEnumerable<AgentTransactionModel> AgentTransactions => _agentTransactions;
		private readonly IDialogueService _dialogueService;
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		[ObservableProperty]
		private AgentTransactionModel? _selectedTransaction;
		[ObservableProperty]
		private AgentModel? _selectedAgent;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		[ObservableProperty]
		private string _agentSearchField = string.Empty;
		[ObservableProperty]
		private DateTime _startDate;
		[ObservableProperty]
		private DateTime _endDate;
		[ObservableProperty]
		private bool _isCurrencyVisible = false;
		[ObservableProperty] 
		private bool _isDebtCreditVisible = false;
		[ObservableProperty]
		private bool _isLoaded = !false;
		[ObservableProperty] 
		private double _totalDebt;
		[ObservableProperty] 
		private double _totalCredit;
		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(AddNewAgentCommand))]
		private bool _isDataBaseLoaded = false;
		public AgentViewModel()
		{
			_agents = new ObservableCollection<AgentModel>();
			_currencyModels = new ObservableCollection<CurrencyModel>();
			_agentTransactions = new ObservableCollection<AgentTransactionModel>();
			_isDebtCreditVisible = true;

			_startDate = DateTime.Now.AddMonths(-1).Date;
			_endDate = DateTime.Now.Date;
		}
		public AgentViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
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

		private void OnDataBaseEdited(object recipient, EditedMessage message)
		{
				string where = message.Value.Where;
				if (where == "Currencies")
				{
					var what = message.Value.What as IEnumerable<CurrencyModel>;
					if (what != null)
					{
						Dispatcher.UIThread.Post(() =>
						{
							_currencyModels.Clear();
							_currencyModels.AddRange(what);
						});
					}
				}
		}

		private void OnAction(object recipient, ActionMessage message)
		{
			if (message.Value == "DataBaseLoaded")
			{
				Dispatcher.UIThread.Post(() =>
				{
					IsLoaded = !true;
					IsDataBaseLoaded = true;
					_agents.AddRange(_dataStore.AgentModels.Where(x => x.Id != 1));
					_currencyModels.AddRange(_dataStore.CurrencyModels);
				});
			}
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
				if (_dataStore.AgentModels.Count() != _agents.Count)
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
			GetTransactionsCommand.Execute(null);
		}
		partial void OnStartDateChanged(DateTime value)
		{
			GetTransactionsCommand.Execute(null);
		}
		[RelayCommand]
		private async Task GetTransactions()
		{
			TotalDebt = 0;
			TotalCredit = 0;
			_agentTransactions.Clear();
			if (SelectedAgent != null && SelectedCurrency != null)
			{
				var model = await _topModel.GetAgentTransactionsByIdsAsync(SelectedAgent.Id, SelectedCurrency.Id ?? default, Converters.ToDateTimeSqlite(StartDate.ToString("dd.MM.yyyy")), Converters.ToDateTimeSqlite(EndDate.ToString("dd.MM.yyyy")));
				_agentTransactions.AddRange(model.Where(x => x.TransactionStatus != 3).OrderByDescending(x => x.Id));
				var lastTr = _agentTransactions.FirstOrDefault();
				if (lastTr != null)
				{
					if (lastTr.Balance > 0)
						TotalDebt = lastTr.Balance;
					else if(lastTr.Balance < 0)
						TotalCredit = lastTr.Balance * -1;
				}
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
			await _dialogueService.OpenDialogue(new AddNewAgentWindow(), new AddNewAgentViewModel(Messenger, _topModel), parent);
		}
		private bool canDoWithAgent() => SelectedAgent != null;
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
			{
				await _dialogueService.OpenDialogue(new AddNewTransactionWindow(), new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id, 0), parent);
			}
		}

		[RelayCommand(CanExecute = nameof(canDoWithAgent))]
		private async Task AddNewConsumption(Window parent)
		{
			if (SelectedAgent != null)
			{
				await _dialogueService.OpenDialogue(new AddNewTransactionWindow(), new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id, 2), parent);
			}
		}
		[RelayCommand(CanExecute = nameof(canDoWithAgent))]
		private async Task AddNewInCome(Window parent)
		{
			if (SelectedAgent != null)
			{
				await _dialogueService.OpenDialogue(new AddNewTransactionWindow(), new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, SelectedAgent.Name, SelectedAgent.Id, 3), parent);
			}
		}
		private bool canDoWithTransaction() => SelectedTransaction != null;
		[RelayCommand(CanExecute = nameof(canDoWithTransaction))]
		private async Task DeleteTransaction(Window parent)
		{
			if (SelectedAgent != null)
			{
				if (SelectedCurrency != null && SelectedTransaction != null && SelectedTransaction.CurrencyId != 1)
				{
					if (SelectedTransaction.TransactionStatus == 2 || SelectedTransaction.TransactionStatus == 4)
					{
						var res = await MessageBoxManager.GetMessageBoxStandard("Удалить закупку/продажу",
							$"Нельзя удалить транзакцию закупки/продажу.\n Для удаления транзакции нужно удалить закупку.").ShowWindowDialogAsync(parent);
					}
					else
					{
						var res = await MessageBoxManager.GetMessageBoxStandard("Удалить транзакцию",
							$"Вы уверенны что хотите удалить транзакцию?",
							ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
						if (res == ButtonResult.Yes)
						{
							await _topModel.DeleteAgentTransactionAsync(SelectedAgent.Id, SelectedTransaction.CurrencyId, SelectedTransaction.Id);
							GetTransactionsCommand.Execute(null);
						}
					}
				}
			}
			
		}
	}
}
