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
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class AddNewTransactionViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private readonly AgentTransactionModel? _transactionData;
		public AgentTransactionModel? TransactionData => _transactionData;
		private int _action = 0;
		public int Action => _action;
		private int _agentId;
		[ObservableProperty]
		private string _actionName = "Транзакция:";
		[ObservableProperty]
		private Decimal _minTrSum = Decimal.MinValue;
		[ObservableProperty]
		private double _transactionSum;
		[ObservableProperty]
		private DateTime _date;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		[ObservableProperty]
		private CurrencyModel? _selectedConvertCurrency;
		[ObservableProperty]
		private string _nameOfAgent = string.Empty;
		[ObservableProperty]
		private bool _isVisAndEnb = false;
		[ObservableProperty]
		private bool _convertFromCurr = false;
		[ObservableProperty]
		private bool _isEnb = true;
		private readonly ObservableCollection<CurrencyModel> _currencies;
		public IEnumerable<CurrencyModel> Currencies => _currencies;
		public AddNewTransactionViewModel() 
		{ 
			_currencies = new ObservableCollection<CurrencyModel>();
			_isVisAndEnb = true;
			Date = DateTime.Now.Date;
		}
		/// <summary>
		/// Для оплаты покупок м продаж.
		/// </summary>
		/// <param name="messenger"></param>
		/// <param name="topModel"></param>
		/// <param name="dataStore"></param>
		/// <param name="transactionData"></param>
		/// <param name="nameOfAgent"></param>
		public AddNewTransactionViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore, AgentTransactionModel transactionData, string nameOfAgent) : base(messenger)
		{
			_action = 1;
			_transactionData = transactionData;
			_isVisAndEnb = false;
			_isEnb = false;
			_topModel = topModel;
			_dataStore = dataStore;
			_nameOfAgent = nameOfAgent;
			_agentId = transactionData.AgentId;
			Date = DateTime.Now.Date;
			TransactionSum = -1 * transactionData.TransactionSum;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
			SelectedCurrency = _currencies.FirstOrDefault(x => x.Id == transactionData.CurrencyId);
		}

		private void OnStart()
		{
			if (_action == 2)
			{
				MinTrSum = 0;
				ActionName = "Расход:";
			}
			else if(_action == 3)
			{
				MinTrSum = 0;
				ActionName = "Приход:";
			}
		}
		/// <summary>
		/// Для создания транзакции.
		/// </summary>
		/// <param name="messenger"></param>
		/// <param name="topModel"></param>
		/// <param name="dataStore"></param>
		/// <param name="nameOfAgent"></param>
		/// <param name="agentId"></param>
		/// <param name="action">2 - Новый расход, 3 - Новый приход</param>
		public AddNewTransactionViewModel(IMessenger messenger, TopModel topModel,DataStore dataStore, string nameOfAgent, int agentId, int action) : base(messenger)
		{
			_action = action;
			_isVisAndEnb = false;
			_topModel = topModel;
			_dataStore = dataStore;
			_nameOfAgent = nameOfAgent;
			_agentId = agentId;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
			Date = DateTime.Now.Date;
			
			OnStart();
		}
		partial void OnTransactionSumChanged(double value)
		{
			TransactionSum = Math.Round(TransactionSum, 2);
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
			if (_action == 0)
			{
				await AddNewTransactionNormal(TransactionSum);
			}
			else if(_action == 2)
			{
				await AddNewTransactionNormal(-1 * TransactionSum);
			}
			else if(_action == 3)
			{
				await AddNewTransactionNormal(TransactionSum);
			}
			else if(_action == 1)
			{
				if (ConvertFromCurr)
				{
					if (SelectedCurrency != null && SelectedConvertCurrency != null)
					{
						int status = 0;
						if (TransactionSum > 0)
							status = 1;
						else if (TransactionSum < 0)
							status = 0;

						AgentTransactionModel lastTransaction = await _topModel.GetLastTransactionAsync(_agentId, SelectedCurrency.Id ?? default);
						double sum = -1 * Math.Round(TransactionSum/ SelectedConvertCurrency.ToUsd * SelectedCurrency.ToUsd, 2);
						await _topModel.AddNewTransactionAsync(new AgentTransactionModel
						{
							AgentId = _agentId,
							CurrencyId = SelectedCurrency.Id ?? default,
							TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
							TransactionStatus = status,
							TransactionSum = sum,
							Balance = lastTransaction.Balance + sum
						});
						Messenger.Send(new ActionMessage("Update"));
					}
				}
				else 
				{
					await AddNewTransactionNormal(TransactionSum);
				}
				
			}
			
		}
		[RelayCommand]
		private async Task AddNewTransactionNormal(double transactionSum)
		{
			if (SelectedCurrency != null)
			{
				int status = 0;
				if (transactionSum > 0)
					status = 1;
				else if (transactionSum < 0)
					status = 0;

				AgentTransactionModel lastTransaction = await _topModel.GetLastTransactionAsync(_agentId, SelectedCurrency.Id ?? default);
				await _topModel.AddNewTransactionAsync(new AgentTransactionModel
				{
					AgentId = _agentId,
					CurrencyId = SelectedCurrency.Id ?? default,
					TransactionDatatime = Date.Date.ToString("dd.MM.yyyy"),
					TransactionStatus = status,
					TransactionSum = transactionSum,
					Balance = lastTransaction.Balance + transactionSum
				});
				Messenger.Send(new ActionMessage("Update"));
			}
		}
	}
}
