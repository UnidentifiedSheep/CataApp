using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels
{
	public partial class CatalogueViewModel : ViewModelBase
	{
		private readonly DataStore _dataStore;
		private CatalogueModel? _selecteditem;
		private readonly ObservableCollection<CatalogueModel> _catalogueModels;
		public HierarchicalTreeDataGridSource<CatalogueModel> CatalogueModels { get; }

		[ObservableProperty]
		private string _partName = string.Empty;
		[ObservableProperty]
		private string _partUniValue = string.Empty;

		
		public CatalogueViewModel(IMessenger messenger, DataStore dataStore) : base(messenger) 
		{
			_dataStore = dataStore;
			_catalogueModels = new ObservableCollection<CatalogueModel>();

			CatalogueModels = new HierarchicalTreeDataGridSource<CatalogueModel>(_catalogueModels)
			{
				Columns =
				{
					new HierarchicalExpanderColumn<CatalogueModel>(
						new TextColumn<CatalogueModel, string>
							("Название", x => x.Name, new GridLength(580)), x => x.Children),
					new TextColumn<CatalogueModel, string>(
						"Номер запчасти", x => x.UniValue, new GridLength(150)),
					new TextColumn<CatalogueModel, string>(
						"Производитель", x => x.ProducerName, new GridLength(130)),
					new TextColumn<CatalogueModel, int>(
						"Количество", x=> x.Count, GridLength.Star),
					new TextColumn<CatalogueModel, double>(
						"Цена", x => x.Price, GridLength.Star)
				}
			};
			Messenger.Register<DataBaseLoadedMessage>(this, OndataBaseLoaded);
		}

		private void OndataBaseLoaded(object recipient, DataBaseLoadedMessage message)
		{
			_catalogueModels.AddRange(_dataStore.CatalogueModels);
		}

		partial void OnPartNameChanged(string value)
		{
			var filter = new AsyncRelayCommand(async (CancellationToken token) =>
			{
				_catalogueModels.Clear();
				await foreach (var res in DataFiltering.FilterByMainName(_dataStore.CatalogueModels, value, token))
					_catalogueModels.Add(res);
			});
			if (value.Length >= 3)
				filter.Execute(null);
			else if (value.Length <= 2 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
			{
				_catalogueModels.Clear();
				_catalogueModels.AddRange(_dataStore.CatalogueModels);
			}	
		}
		partial void OnPartUniValueChanged(string value)
		{
			var filter = new AsyncRelayCommand(async (CancellationToken token) =>
			{
				_catalogueModels.Clear();
				await foreach (var res in DataFiltering.FilterByUniValue(_dataStore.CatalogueModels, value, token))
					_catalogueModels.Add(res);
			});
			if (value.Length >= 3)
				filter.Execute(null);
			else if (value.Length <= 2 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
			{
				_catalogueModels.Clear();
				_catalogueModels.AddRange(_dataStore.CatalogueModels);
			}
		}
		[RelayCommand]
		private async Task DeleteGroup()
		{

		}
		[RelayCommand]
		private async Task DeleteSolo()
		{

		}
		[RelayCommand]
		private async Task EditCatalogue()
		{

		}
	}

}
