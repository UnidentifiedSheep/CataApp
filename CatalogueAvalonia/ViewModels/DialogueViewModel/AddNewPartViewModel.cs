using CatalogueAvalonia.Core;
using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class AddNewPartViewModel : ViewModelBase
	{
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		[ObservableProperty]
		private string _nameOfParts = string.Empty;
		[ObservableProperty]
		private string _parts = string.Empty;
		[ObservableProperty]
		private string _producerSearchField = string.Empty;
		[ObservableProperty]
		private CatalogueModel? _selectedCatalogue;
		[ObservableProperty]
		private ProducerModel? _selectedProducer;
		private readonly ObservableCollection<CatalogueModel> _catalogueModels;
		public IEnumerable<CatalogueModel> Catalogues => _catalogueModels;
		private readonly ObservableCollection<ProducerModel> _producers;
		public IEnumerable<ProducerModel> Producers => _producers;
		public AddNewPartViewModel() 
		{
			_catalogueModels = new ObservableCollection<CatalogueModel>();
			_producers = new ObservableCollection<ProducerModel>();
		}
		public AddNewPartViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel) : base(messenger)
		{
			_dataStore = dataStore;
			_topModel = topModel;
			_catalogueModels = new ObservableCollection<CatalogueModel>();
			_producers = new ObservableCollection<ProducerModel>(_dataStore.ProducerModels);
		}
		List<string> parts = new List<string>();
		partial void OnPartsChanged(string value)
		{
			var filterSlashes = new AsyncRelayCommand(async () =>
			{
				parts = await DataFiltering.FilterSlashes(value);
			});
			if (value.Length >= 2)
			{
				parts.Clear();
				_catalogueModels.Clear();
				filterSlashes.Execute(null);
				_catalogueModels.AddRange(parts.Select(x => new CatalogueModel
				{
					UniValue = x,
					ProducerId = 0,
					ProducerName = "Неизвестный"
				}));
			
			}
			else
				_catalogueModels.Clear();

		}
		[RelayCommand]
		private async Task filterProducers(string value)
		{
			await foreach (var item in DataFiltering.FilterProducer(_dataStore.ProducerModels, value))
				_producers.Add(item);
		}
		partial void OnProducerSearchFieldChanged(string value)
		{
			_producers.Clear();
			filterProducersCommand.Execute(value);
				
		}
		partial void OnSelectedCatalogueChanged(CatalogueModel? value)
		{
			if (value != null && SelectedProducer != null)
			{
				var index = _catalogueModels.IndexOf(value);
				ChangeProducer(index, SelectedProducer.Id, SelectedProducer.ProducerName);
			}
		}
		partial void OnSelectedProducerChanged(ProducerModel? value)
		{
			if (value != null && SelectedCatalogue != null)
			{
				var index = _catalogueModels.IndexOf(SelectedCatalogue);
				ChangeProducer(index, value.Id, value.ProducerName);
			}
		}
		private void ChangeProducer(int index, int id, string name)
		{
			if (SelectedCatalogue != null && SelectedProducer != null)
			{
				SelectedCatalogue = null;
				SelectedProducer = null;
				_catalogueModels[index].ProducerId = id;
				_catalogueModels[index].ProducerName = name;
			}
		}
		[RelayCommand]
		private async Task AddToCatalogue()
		{
			var newId = await _topModel.AddNewCatalogue(new CatalogueModel
			{
				Name = NameOfParts,
				Children = _catalogueModels
			});

			Messenger.Send(new AddedMessage(new ChangedItem { Id = newId, Where = "Catalogue", What = await _topModel.GetCatalogueByIdAsync(newId)}));
		}
	}
}
