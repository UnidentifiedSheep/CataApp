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
		private readonly AgentTransactionModel? _transactionData;
		public AgentTransactionModel? TransactionData => _transactionData;
		private int action = 0;
		public int Action => action;
		private int _agentId;
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
			action = 1;
			_transactionData = transactionData;
			_isVisAndEnb = false;
			_isEnb = false;
			_topModel = topModel;
			_dataStore = dataStore;
			_nameOfAgent = nameOfAgent;
			_agentId = transactionData.AgentId;
			Date = DateTime.Now.Date;
			TransactionSum = -1 * transactionData.TransactionSum;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 0));
			SelectedCurrency = _currencies.Where(x => x.Id == transactionData.CurrencyId).FirstOrDefault();
		}
		/// <summary>
		/// Для создания транзакции.
		/// </summary>
		/// <param name="messenger"></param>
		/// <param name="topModel"></param>
		/// <param name="dataStore"></param>
		/// <param name="nameOfAgent"></param>
		/// <param name="agentId"></param>
		public AddNewTransactionViewModel(IMessenger messenger, TopModel topModel,DataStore dataStore, string nameOfAgent, int agentId) : base(messenger)
		{
			action = 0;
			_isVisAndEnb = false;
			_topModel = topModel;
			_dataStore = dataStore;
			_nameOfAgent = nameOfAgent;
			_agentId = agentId;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 0));
			Date = DateTime.Now.Date;
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
			if (action == 0)
			{
				await AddNewTransactionNormal(TransactionSum);
			}
			else if(action == 1)
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
