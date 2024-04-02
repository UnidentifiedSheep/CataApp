using Avalonia.Controls;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
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
using System.Text;
using System.Threading.Tasks;
using CatalogueAvalonia.Services.Messeges;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class EditPricesViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private readonly int _mainCatId;
		public bool IsDirty = false;
		private readonly ObservableCollection<MainCatPriceModel> _mainCatPrices;
		public IEnumerable<MainCatPriceModel> MainCatPrices => _mainCatPrices;
		private List<CurrencyModel> _currencies;
		[ObservableProperty]
		private MainCatPriceModel? _selectedPrice;
		[ObservableProperty]
		private string _uniValue = string.Empty;
		public EditPricesViewModel()
		{
			_mainCatPrices = new ObservableCollection<MainCatPriceModel>();
			_currencies = new List<CurrencyModel>();
			_uniValue = "Номер запчасти";
		}
		public EditPricesViewModel(IMessenger messenger, TopModel topModel, int mainCatId, DataStore dataStore, string uniValue) : base(messenger)
		{
			_mainCatId = mainCatId;
			_topModel = topModel;
			_dataStore = dataStore;
			_uniValue = uniValue;
			_mainCatPrices = new ObservableCollection<MainCatPriceModel>();
			_currencies = new List<CurrencyModel>(_dataStore.CurrencyModels);

			GetPricesCommand.Execute(null);
		}
		[RelayCommand]
		private async Task SaveChanges()
		{
			BeforeSave();
			CatalogueModel? model = await _topModel.EditMainCatPricesAsync(_mainCatPrices, _mainCatId);
			if (_mainCatPrices.Any() && (_mainCatPrices.Where(x => x.IsDirty).Any() || IsDirty))
			{
				Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePrices", Id = _mainCatId, What = model}));
			}
			else if(!_mainCatPrices.Any() && (_mainCatPrices.Where(x => x.IsDirty).Any() || IsDirty) && model != null)
			{
				Messenger.Send(new DeletedMessage(new DeletedItem { Id = _mainCatId, Where = "CataloguePrices" , SecondId = model.UniId}));
			}
		}
		private void BeforeSave()
		{
			var usdCurrency = _currencies.Where(x => x.Id == 2).SingleOrDefault();

			_mainCatPrices.RemoveMany(_mainCatPrices.Where(x => x.Count <= 0 || x.Price <= 0));
			foreach (var item in _mainCatPrices) 
			{
				if (item.SelectedCurrency == null)
					item.SelectedCurrency = usdCurrency;


				if (item.SelectedCurrency != null && item.CurrencyId != 2)
				{
					item.Price = Math.Round(item.Price / item.SelectedCurrency.ToUsd, 2);
					item.SelectedCurrency = usdCurrency;
				}
			}
			_mainCatPrices.RemoveMany(_mainCatPrices.Where(x => x.Count <= 0 || x.Price <= 0));
		}
		[RelayCommand]
		private async Task GetPrices()
		{
			_mainCatPrices.AddRange(await _topModel.GetMainCatPricesByIdAsync(_mainCatId));
			foreach (var item in _mainCatPrices)
			{
				item.Currency = new(_currencies.Where(x => x.Id != 0));
				item.SelectedCurrency = item.Currency.Where(x => x.Id == item.CurrencyId).SingleOrDefault();
			}
			IsDirty = false;
		}
		[RelayCommand]
		private void AddNewPrice()
		{
			_mainCatPrices.Add(new MainCatPriceModel()
			{ 
				Id = null,
				Count = 0,
				Currency = new(_currencies.Where(x => x.Id != 0)),
				CurrencyId = 0,
				MainCatId = _mainCatId,
				Price = 0,
				IsDirty = false
			});
			IsDirty = true;
		}
		[RelayCommand]
		private void RemovePrice() 
		{
			if (SelectedPrice != null)
			{
				_mainCatPrices.Remove(SelectedPrice);
				IsDirty = true;
			}
		}
	}
}
