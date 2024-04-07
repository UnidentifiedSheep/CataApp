using Avalonia.Controls;
using CatalogueAvalonia.Core;
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
	public partial class NewProdajaViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private readonly IDialogueService _dialogueService;
		private readonly ObservableCollection<CurrencyModel> _currencies;
		private readonly ObservableCollection<AgentModel> _agents;
		private readonly ObservableCollection<ProdajaAltModel> _prodajaAlts;

		public IEnumerable<CurrencyModel> Currencies => _currencies;
		public IEnumerable<AgentModel> Agents => _agents;
		public IEnumerable<ProdajaAltModel> Prodaja => _prodajaAlts;

		[ObservableProperty]
		private DateTime _prodajaDate;
		[ObservableProperty]
		private double _totalSum;
		[ObservableProperty]
		private AgentModel? _selectedAgent;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		[ObservableProperty]
		private ProdajaAltModel? _selectedProdaja;
		[ObservableProperty]
		private bool _isVisibleConverter = true;
		[ObservableProperty]
		string _comment = string.Empty;
		[ObservableProperty] 
		private int _overPrice = 20;
		public NewProdajaViewModel() 
		{
			_currencies = new ObservableCollection<CurrencyModel>();
			_agents = new ObservableCollection<AgentModel>();
			_prodajaAlts = new ObservableCollection<ProdajaAltModel>();
			_prodajaDate = DateTime.Now.Date;
		}
		public NewProdajaViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore, IDialogueService dialogueService) : base(messenger)
		{
			_topModel = topModel;
			_dataStore = dataStore;
			_dialogueService = dialogueService;
			
			_prodajaDate = DateTime.Now.Date;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
			_agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.Id != 1));
			_prodajaAlts = new ObservableCollection<ProdajaAltModel>();

			Messenger.Register<AddedMessage>(this, OnItemAdded);
			
		}

		private void OnItemAdded(object recipient, AddedMessage message)
		{
			var where = message.Value.Where;
			if (where == "CataloguePartItemSelected")
			{
				double price = 0;
				var what = (CatalogueModel?)message.Value.What;
				string mainName = message.Value.MainName;
				if (what != null)
				{
					if (!_prodajaAlts.Any(x => x.MainCatId == what.MainCatId))
					{
						if (what.Children != null && what.Children.Any())
						{
							var firstPrice = what.Children.First();
							var partsCurr = _dataStore.CurrencyModels.FirstOrDefault(x => x.Id == firstPrice.CurrencyId);

							if (partsCurr != null && SelectedCurrency != null)
								price = Math.Round(firstPrice.Price / partsCurr.ToUsd * SelectedCurrency.ToUsd * (100+OverPrice)/100, 2);
						}
						
						_prodajaAlts.Add(new ProdajaAltModel
						{
							MainCatId = what.MainCatId,
							MainCatName = what.Name,
							MainName = mainName,
							MaxCount = what.Count,
							UniValue = what.UniValue,
							Price = price
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
					if (!_prodajaAlts.Any(x => x.MainCatId == what.MainCatId))
					{
						if (SelectedProdaja != null)
						{
							SelectedProdaja.MainCatId = what.MainCatId;
							SelectedProdaja.MainCatName = what.Name;
							SelectedProdaja.MainName = mainName;
							SelectedProdaja.UniValue = what.UniValue;
							SelectedProdaja.MaxCount = what.Count;
							SelectedProdaja.Count = 0;
						}
					}
					
				}
			}
		}
		[RelayCommand]
		private async Task AddNewPart(Window parent)
		{
			SelectedProdaja = null;
			await _dialogueService.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 0), parent);
		}
		[RelayCommand]
		private async Task ChangePart(Window parent)
		{
			await _dialogueService.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 1), parent);
		}
		[RelayCommand]
		private void DeletePart(Window parent)
		{
			if (SelectedProdaja != null)
			{
				_prodajaAlts.Remove(SelectedProdaja);
			}
		}
		public void RemoveWhereZero(IEnumerable<ProdajaAltModel> models)
		{
			_prodajaAlts.RemoveMany(models);
		}
		[RelayCommand]
		private async Task SaveChanges(Window parent)
		{
			if (SelectedAgent != null && SelectedCurrency != null)
			{
				AgentTransactionModel lastTransaction = await _topModel.GetLastTransactionAsync(SelectedAgent.Id, SelectedCurrency.Id ?? default);
				AgentTransactionModel agentModel = new AgentTransactionModel 
				{
					AgentId = SelectedAgent.Id,
					CurrencyId = SelectedCurrency.Id ?? default,
					TransactionDatatime = ProdajaDate.Date.ToString("dd.MM.yyyy"),
					TransactionSum = Math.Round(TotalSum, 2),
					Balance = lastTransaction.Balance + Math.Round(TotalSum, 2),
					TransactionStatus = 4
				};
				int transactionId = await _topModel.AddNewTransactionAsync(agentModel);
				await _dialogueService.OpenDialogue(new AddNewPayment(), new AddNewTransactionViewModel(Messenger, _topModel, _dataStore, agentModel, SelectedAgent.Name), parent);
				parent.Close();
				ProdajaModel mainModel = new ProdajaModel
				{
					AgentId = SelectedAgent.Id,
					Datetime = Converters.ToDateTimeSqlite(ProdajaDate.Date.ToString("dd.MM.yyyy")),
					Comment = Comment,
					CurrencyId = SelectedCurrency.Id ?? 0,
					TotalSum = TotalSum,
					TransactionId = transactionId,
				};
				var catas = await _topModel.AddNewProdaja(Prodaja, mainModel);
				Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
				Messenger.Send(new ActionMessage("Update"));
			}
		}
	}
}
