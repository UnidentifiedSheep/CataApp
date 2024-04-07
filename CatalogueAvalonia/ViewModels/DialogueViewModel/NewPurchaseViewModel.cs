using Avalonia.Controls;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class NewPurchaseViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private readonly IDialogueService _dialogueServices;
		private readonly ObservableCollection<CurrencyModel> _currencies;
		private readonly ObservableCollection<AgentModel> _agents;
		private readonly ObservableCollection<ZakupkaAltModel> _zakupka;
		public IEnumerable<CurrencyModel> Currencies => _currencies;
		public IEnumerable<AgentModel> Agents => _agents;
		public IEnumerable<ZakupkaAltModel> Zakupka => _zakupka;

		[ObservableProperty]
		private DateTime _purchaseDate;
		[ObservableProperty]
		private double _toUsd;
		[ObservableProperty]
		private bool _convertToUsd = true;
		[ObservableProperty]
		private bool _canEditUsd;
		[ObservableProperty]
		private double _totalSum;
		[ObservableProperty]
		private AgentModel? _selectedAgent;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		[ObservableProperty]
		private ZakupkaAltModel? _selectedZakupka;
		[ObservableProperty]
		private bool _isVisibleConverter = true;
		[ObservableProperty]
		private string _comment = String.Empty;
		public NewPurchaseViewModel()
		{
			_purchaseDate = DateTime.Now.Date;
			_currencies = new ObservableCollection<CurrencyModel>();
			_agents = new ObservableCollection<AgentModel>();
			_zakupka = new ObservableCollection<ZakupkaAltModel>();
			_canEditUsd = !ConvertToUsd;
		}
		public NewPurchaseViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
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

		private void OnItemAdded(object recipient, AddedMessage message)
		{
			var where = message.Value.Where;
			if (where == "CataloguePartItemSelected")
			{
				var what = (CatalogueModel?)message.Value.What;
				string mainName = message.Value.MainName;
				if (what != null)
				{
					if (!_zakupka.Any(x => x.MainCatId == what.MainCatId))
					{
						_zakupka.Add(new ZakupkaAltModel
						{
							MainCatId = what.MainCatId,
							MainCatName = what.Name,
							MainName = mainName,
							UniValue = what.UniValue,
						});
					}
				}
			}
			else if (where == "ZakupkaPartItemEdited")
			{
				var what = (CatalogueModel?)message.Value.What;
				string mainName = message.Value.MainName;
				if (what != null)
				{
					if (SelectedZakupka != null)
					{
						SelectedZakupka.MainCatId = what.MainCatId;
						SelectedZakupka.MainCatName = what.Name;
						SelectedZakupka.MainName = mainName;
						SelectedZakupka.UniValue = what.UniValue;
					}
				}
			}
		}
		partial void OnSelectedCurrencyChanged(CurrencyModel? value)
		{
			if (value != null)
			{
				ToUsd = value.ToUsd;
			}
		}
		partial void OnToUsdChanged(double value)
		{
			if (SelectedCurrency != null && SelectedCurrency.Id == 2)
			{
				ToUsd = 1;
			}
		}
		partial void OnConvertToUsdChanged(bool value)
		{
			CanEditUsd = !ConvertToUsd;
		}
		[RelayCommand]
		private async Task AddNewPart(Window parent)
		{
			SelectedZakupka = null;
			await _dialogueServices.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 0), parent);
		}
		[RelayCommand]
		private void DeletePart(Window parent)
		{
			if(SelectedZakupka != null) 
			{
				_zakupka.Remove(SelectedZakupka);
			}
		}
		[RelayCommand]
		private async Task ChangePart(Window parent)
		{
			await _dialogueServices.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 1), parent);
		}
		public void RemoveWhereZero(IEnumerable<ZakupkaAltModel> altModels)
		{
			_zakupka.RemoveMany(altModels);
		}
		[RelayCommand]
		private async Task SaveZakupka(Window parent)
		{
			if (SelectedAgent != null && SelectedCurrency != null)
			{
				var transactionSum = -1 * TotalSum;
				AgentTransactionModel lastTransaction = await _topModel.GetLastTransactionAsync(SelectedAgent.Id, SelectedCurrency.Id ?? default);
				var agentModel = new AgentTransactionModel
				{
					AgentId = SelectedAgent.Id,
					CurrencyId = SelectedCurrency.Id ?? default,
					TransactionDatatime = PurchaseDate.Date.ToString("dd.MM.yyyy"),
					TransactionStatus = 2,
					TransactionSum = transactionSum,
					Balance = lastTransaction.Balance + transactionSum,
				};
				int transactionId = await _topModel.AddNewTransactionAsync(agentModel);
				await _dialogueServices.OpenDialogue(new AddNewPayment(), new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, agentModel, SelectedAgent.Name), parent);
				parent.Close();
				var zakMain = new ZakupkiModel
				{
					AgentId = SelectedAgent.Id,
					CurrencyId = SelectedCurrency.Id ?? default,
					Datetime = PurchaseDate.Date.ToString("dd.MM.yyyy"),
					TransactionId = transactionId,
					TotalSum = TotalSum,
					Comment = Comment
				};
				await _topModel.AddNewZakupkaAsync(_zakupka, zakMain);

				if (ConvertToUsd)
				{
					var zakupkiToUsd = _zakupka.Select(x => new ZakupkaAltModel
					{
						Count = x.Count,
						MainCatId = x.MainCatId,
						MainName = x.MainName,
						PriceSum = x.PriceSum,
						MainCatName = x.MainCatName,
						UniValue = x.UniValue,
						Price = Math.Round(x.Price / SelectedCurrency.ToUsd, 2),
					});
					var catas = await _topModel.AddNewPricesForPartsAsync(zakupkiToUsd, 2);
					Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
					Messenger.Send(new ActionMessage("Update"));
				}
				else
				{
					var catas = await _topModel.AddNewPricesForPartsAsync(_zakupka, SelectedCurrency.Id ?? 2);
					Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
					Messenger.Send(new ActionMessage("Update"));
				}
			}
		}
	}
}
