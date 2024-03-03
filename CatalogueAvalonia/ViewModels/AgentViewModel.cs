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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
		public AgentViewModel()
		{
			_agents = new ObservableCollection<AgentModel>();
			_currencyModels = new ObservableCollection<CurrencyModel>();
			_agentTransactions = new ObservableCollection<AgentTransactionModel>();

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

			Messenger.Register<DataBaseLoadedMessage>(this, OnDataBaseLoaded);
			Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
		}

		private void OnDataBaseAdded(object recipient, AddedMessage message)
		{
			if (message.Value.Where == "Agent")
			{
				var what = (AgentModel?)message.Value.What;
				if (what != null)
					_agents.Add(what);
			}
		}

		private void OnDataBaseLoaded(object recipient, DataBaseLoadedMessage message)
		{
			_agents.AddRange(_dataStore.AgentModels);
			_currencyModels.AddRange(_dataStore.CurrencyModels);
		}
		[RelayCommand]
		private async Task FilterAgent(string value)
		{
			_agents.Clear();
			await foreach (var agent in DataFiltering.FilterAgents(_dataStore.AgentModels, value))
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
					_agents.AddRange(_dataStore.AgentModels);
				}
			}

		}
		partial void OnSelectedCurrencyChanged(CurrencyModel? value)
		{
			if (SelectedCurrency != null && SelectedCurrency.Id == 0)
				IsCurrencyVisible = true;
			else
				IsCurrencyVisible = false;
			GetTransactionsCommand.Execute(value);
		}
		partial void OnSelectedAgentChanged(AgentModel? value)
		{
			GetTransactionsCommand.Execute(value);
		}
		partial void OnEndDateChanged(DateTime value)
		{
			GetTransactionsCommand.Execute(value);
		}
		partial void OnStartDateChanged(DateTime value)
		{
			GetTransactionsCommand.Execute(value);
		}
		[RelayCommand]
		private async Task GetTransactions()
		{
			_agentTransactions.Clear();
			if (SelectedAgent != null && SelectedCurrency != null)
				_agentTransactions.AddRange(await _topModel.GetAgentTransactionsByIdsAsync(SelectedAgent.Id, SelectedCurrency.Id, Converters.ToDateTimeSqlite(StartDate.ToString("dd.MM.yyyy")), Converters.ToDateTimeSqlite(EndDate.ToString("dd.MM.yyyy"))));
		}
		[RelayCommand]
		private async Task EditAgent()
		{
			if (SelectedAgent != null)
			{
				await _topModel.EditAgentAsync(SelectedAgent);
				Messenger.Send(new EditedMessage(new ChangedItem { Id = SelectedAgent.Id, Where = "Agents", What = SelectedAgent }));
			}
		}
		[RelayCommand]
		private async Task AddNewAgent(Window parent)
		{
			await _dialogueService.OpenDialogue(new AddNewAgentWindow(), new AddNewAgentViewModel(Messenger, _topModel), parent);
		}
	}
}
