using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class AddNewTransactionViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private int _agentId;
		[ObservableProperty]
		private double _transactionSum;
		[ObservableProperty]
		private DateTime _date;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		[ObservableProperty]
		private string _nameOfAgent = string.Empty;

		private readonly ObservableCollection<CurrencyModel> _currencies;
		public IEnumerable<CurrencyModel> Currencies => _currencies;
		public AddNewTransactionViewModel() 
		{ 
			_currencies = new ObservableCollection<CurrencyModel>();
			Date = DateTime.Now.Date;
		}
		public AddNewTransactionViewModel(IMessenger messenger, TopModel topModel,DataStore dataStore, string nameOfAgent, int agentId) : base(messenger)
		{
			_topModel = topModel;
			_dataStore = dataStore;
			_nameOfAgent = nameOfAgent;
			_agentId = agentId;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 0));
			Date = DateTime.Now.Date;
		}
		[RelayCommand]
		private async Task AddTransaction()
		{
			if (SelectedCurrency != null)
			{
				int status = 0;
				if (TransactionSum > 0)
					status = 1;
				else if(TransactionSum < 0)
					status = 0;

				AgentTransactionModel lastTransaction = await _topModel.GetLastTransactionAsync(_agentId, SelectedCurrency.Id ?? default);
				await _topModel.AddNewTransactionAsync(new AgentTransactionModel
				{
					AgentId = _agentId,
					CurrencyId = SelectedCurrency.Id ?? default,
					TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
					TransactionStatus = status,
					TransactionSum = TransactionSum,
					Balance = lastTransaction.Balance + TransactionSum
				});
				Messenger.Send(new ActionMessage("Update"));
			}
			
		}
	}
}
