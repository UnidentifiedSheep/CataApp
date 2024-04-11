﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class EditPricesViewModel : ViewModelBase
	{
		private readonly TopModel _topModel;
		private readonly DataStore _dataStore;
		private int _totalCountStart = 0;
		private readonly int _mainCatId;
		public bool IsDirty = false;
		private readonly ObservableCollection<MainCatPriceModel> _mainCatPrices;
		public IEnumerable<MainCatPriceModel> MainCatPrices => _mainCatPrices;
		private List<CurrencyModel> _currencies;
		[ObservableProperty] private MainCatPriceModel? _selectedPrice;
		[ObservableProperty] private string _uniValue = string.Empty;
		[ObservableProperty] private bool _isVisible;
		[ObservableProperty] private bool _isReadOnly;

		public EditPricesViewModel()
		{
			_mainCatPrices = new ObservableCollection<MainCatPriceModel>();
			_currencies = new List<CurrencyModel>();
			_uniValue = "Номер запчасти";
			_isVisible = true;
		}

		/// <summary>
		/// To edit and act with prices.
		/// </summary>
		/// <param name="messenger"></param>
		/// <param name="topModel"></param>
		/// <param name="mainCatId"></param>
		/// <param name="dataStore"></param>
		/// <param name="uniValue"></param>
		public EditPricesViewModel(IMessenger messenger, TopModel topModel, int mainCatId, DataStore dataStore,
			string uniValue) : base(messenger)
		{
			_mainCatId = mainCatId;
			_topModel = topModel;
			_dataStore = dataStore;
			_uniValue = uniValue;
			_mainCatPrices = new ObservableCollection<MainCatPriceModel>();
			_currencies = new List<CurrencyModel>(_dataStore.CurrencyModels);
			_isVisible = true;
			_isReadOnly = false;

			GetPricesCommand.Execute(null);
		}

		/// <summary>
		/// To read-only prices.
		/// </summary>
		/// <param name="messenger"></param>
		/// <param name="topModel"></param>
		/// <param name="dataStore"></param>
		/// <param name="mainCatId"></param>
		/// <param name="uniValue"></param>
		public EditPricesViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore, int mainCatId,
			string uniValue) : base(messenger)
		{
			_mainCatId = mainCatId;
			_topModel = topModel;
			_dataStore = dataStore;
			_uniValue = uniValue;
			_mainCatPrices = new ObservableCollection<MainCatPriceModel>();
			_currencies = new List<CurrencyModel>(_dataStore.CurrencyModels);
			_isVisible = false;
			_isReadOnly = true;

			GetPricesCommand.Execute(null);
		}

		[RelayCommand]
		private async Task SaveChanges()
		{
			BeforeSave();
			var endCount = _mainCatPrices.Sum(x => x.Count) - _totalCountStart;
			CatalogueModel? model = await _topModel.EditMainCatPricesAsync(_mainCatPrices, _mainCatId, endCount);
			if (_mainCatPrices.Any() && (_mainCatPrices.Where(x => x.IsDirty).Any() || IsDirty))
			{
				Messenger.Send(new EditedMessage(new ChangedItem
					{ Where = "CataloguePrices", Id = _mainCatId, What = model }));
			}
			else if (!_mainCatPrices.Any() && (_mainCatPrices.Where(x => x.IsDirty).Any() || IsDirty) && model != null)
			{
				Messenger.Send(new DeletedMessage(new DeletedItem
					{ Id = _mainCatId, Where = "CataloguePrices", SecondId = model.UniId }));
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
					item.Price = item.Price / item.SelectedCurrency.ToUsd;
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
				_totalCountStart += item.Count;
				item.Currency = new ObservableCollection<CurrencyModel>(_currencies.Where(x => x.Id != 1));
				item.SelectedCurrency = item.Currency.SingleOrDefault(x => x.Id == item.CurrencyId);
				item.IsEnabled = IsVisible;
			}

			IsDirty = false;
		}

		[RelayCommand]
		private void AddNewPrice()
		{
			_mainCatPrices.Add(new MainCatPriceModel
			{
				Id = null,
				Count = 0,
				Currency = new ObservableCollection<CurrencyModel>(_currencies.Where(x => x.Id != 1)),
				CurrencyId = 1,
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
