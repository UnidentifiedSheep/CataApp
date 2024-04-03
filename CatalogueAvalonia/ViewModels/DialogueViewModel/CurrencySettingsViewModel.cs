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
	public partial class CurrencySettingsViewModel : ViewModelBase
	{
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		private readonly ObservableCollection<CurrencyModel> _currencyModels;
		private List<int> deletedIds = new List<int>();
		public IEnumerable<CurrencyModel> CurrencyModels => _currencyModels;
		[ObservableProperty]
		private CurrencyModel? _selectedCurrency;
		public bool IsDirty = false;
		public CurrencySettingsViewModel() 
		{ 
			_currencyModels = new ObservableCollection<CurrencyModel>();
		}
		public CurrencySettingsViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel) :base(messenger)
		{
			_dataStore = dataStore;
			_topModel = topModel;
			_currencyModels = new ObservableCollection<CurrencyModel>();
			LoadCurrencyCommand.Execute(null);
		}
		[RelayCommand]
		private async Task LoadCurrency()
		{
			var currencys = await _topModel.GetAllCurrenciesAsync();
			_currencyModels.AddRange(currencys.Where(x => x.Id != 1));
		}
		[RelayCommand]
		private void AddNewCurrency()
		{
			_currencyModels.Add(new CurrencyModel
			{
				CanDelete = 1,
				ToUsd = 1,
			});
			IsDirty = true;
		}
		[RelayCommand]
		private void RemoveCurrency() 
		{
			if (SelectedCurrency != null && SelectedCurrency.CanDelete != 0)
			{
				if (SelectedCurrency.Id != null)
					deletedIds.Add(SelectedCurrency.Id ?? 1);
				_currencyModels.Remove(SelectedCurrency);
				IsDirty = true;
			}
		}
		[RelayCommand]
		private async Task SaveChanges(Window parent)
		{
			var toUsdZero = _currencyModels.Where(x => (x.ToUsd <= 0 || string.IsNullOrEmpty(x.CurrencyName)) && x.CanDelete == 1).ToList();
			if (toUsdZero.Any())
			{
				await MessageBoxManager.GetMessageBoxStandard("?",
						$"Валюты где не было указано название или отношение к 'Доллару' будут удалены.",
						ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
				_currencyModels.Remove(toUsdZero);
			}
			await _topModel.EditCurrenciesAsync(_currencyModels, deletedIds);

			Messenger.Send(new EditedMessage(new ChangedItem { Id = null, Where = "Currencies", What = await _topModel.GetAllCurrenciesAsync() }));
		}
	}
}
