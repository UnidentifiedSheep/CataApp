using Avalonia.Controls;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
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
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Views.DialogueWindows;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class EditPurchaseViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private readonly IDialogueService _dialogueServices;
		private readonly ObservableCollection<CurrencyModel> _currencies;
		private readonly ObservableCollection<AgentModel> _agents;
		private readonly ObservableCollection<ZakupkaAltModel> _zakupka;
		private readonly ZakupkiModel _zakupkaMainGroup;

		private List<int> deletedIds = new List<int>();
		private Dictionary<int, int> prevCounts = new Dictionary<int, int>();

		public IEnumerable<CurrencyModel> Currencies => _currencies;
		public IEnumerable<AgentModel> Agents => _agents;
		public IEnumerable<ZakupkaAltModel> Zakupka => _zakupka;
		public bool IsDirty = false;

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
		private bool _isVisibleConverter = false;
		public EditPurchaseViewModel()
		{
			_purchaseDate = DateTime.Now.Date;
			_currencies = new ObservableCollection<CurrencyModel>();
			_agents = new ObservableCollection<AgentModel>();
			_zakupka = new ObservableCollection<ZakupkaAltModel>();
			_canEditUsd = !ConvertToUsd;
		}
		public EditPurchaseViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService, ZakupkiModel zakupkaMainGroup) : base(messenger)
		{
			_zakupkaMainGroup = zakupkaMainGroup;
			_dataStore = dataStore;
			_topModel = topModel;
			_dialogueServices = dialogueService;
			DateTime.TryParse(zakupkaMainGroup.Datetime, out _purchaseDate);
			_canEditUsd = !ConvertToUsd;
			_currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 0));
			_agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.IsZak == 1 && x.Id != 0));
			_zakupka = new ObservableCollection<ZakupkaAltModel>();
			Messenger.Register<AddedMessage>(this, OnItemAdded);

			LoadZakupkiCommand.Execute(null);
		}
		[RelayCommand]
		private async Task LoadZakupki()
		{
			_zakupka.AddRange(await _topModel.GetZakAltGroup(_zakupkaMainGroup.Id));
			foreach (var item in _zakupka)
			{
				if (!prevCounts.ContainsKey(item.MainCatId ?? 0))
				{
					prevCounts.Add(item.MainCatId ?? 0, item.Count);
				}
				else
				{
					prevCounts[item.MainCatId ?? 0] += item.Count;
				}
			}
			if (_currencies.Where(x => x.Id == _zakupkaMainGroup.CurrencyId).Any())
				SelectedCurrency = _currencies.First(x => x.Id == _zakupkaMainGroup.CurrencyId);
			if (_agents.Where(x => x.Id == _zakupkaMainGroup.AgentId).Any())
				SelectedAgent = _agents.First(x => x.Id == _zakupkaMainGroup.AgentId);
			TotalSum = Math.Round(_zakupka.Sum(x => x.PriceSum), 2);

			IsDirty = false;
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
					_zakupka.Add(new ZakupkaAltModel
					{
						Id = null,
						MainCatId = what.MainCatId,
						MainCatName = what.Name,
						MainName = mainName,
						UniValue = what.UniValue,
						ZakupkaId = _zakupkaMainGroup.Id
					}) ;
					IsDirty = true;
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
						if (SelectedZakupka.Id != null && !deletedIds.Contains(SelectedZakupka.Id ?? 0))
						{
							deletedIds.Add(SelectedZakupka.Id ?? 0);
						}
						SelectedZakupka.Id = null;
						SelectedZakupka.MainCatId = what.MainCatId;
						SelectedZakupka.MainCatName = what.Name;
						SelectedZakupka.MainName = mainName;
						SelectedZakupka.UniValue = what.UniValue;
					}
					IsDirty = true;
				}
			}
		}
		partial void OnPurchaseDateChanged(DateTime value)
		{
			IsDirty = true;
		}
		partial void OnSelectedCurrencyChanged(CurrencyModel? value)
		{
			if (value != null)
			{
				ToUsd = value.ToUsd;
				IsDirty = true;
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
			IsDirty = true;
		}
		[RelayCommand]
		private async Task AddNewPart(Window parent)
		{
			SelectedZakupka = null;
			await _dialogueServices.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 0), parent);
		}
		[RelayCommand]
		private void DeletePart()
		{
			if (SelectedZakupka != null)
			{
				if (SelectedZakupka.Id != null)
				{
					deletedIds.Add(SelectedZakupka.Id ?? 0);
				}
				_zakupka.Remove(SelectedZakupka);
				IsDirty = true;
			}
				TotalSum = Math.Round(_zakupka.Sum(x => x.PriceSum), 2);
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
		private async Task SaveZakupka()
		{
			if (SelectedCurrency != null)
			{
				var catas = await _topModel.EditZakupkaAsync(deletedIds, Zakupka, prevCounts, SelectedCurrency, TotalSum, Converters.ToDateTimeSqlite(PurchaseDate.Date.ToString("dd.MM.yyyy")), _zakupkaMainGroup.TransactionId);
				Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
				Messenger.Send(new ActionMessage("Update"));
			}
		}
		[RelayCommand]
		private async Task DeleteAll()
		{
			IEnumerable<CatalogueModel> catas = new List<CatalogueModel>();
				
			catas = await _topModel.DeleteZakupkaWithPricesReCount(_zakupkaMainGroup.TransactionId, prevCounts.Select(x => new ZakupkaAltModel { MainCatId = x.Key, Count = x.Value}));

			Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
			Messenger.Send(new ActionMessage("Update"));
			
		}
	}
}
