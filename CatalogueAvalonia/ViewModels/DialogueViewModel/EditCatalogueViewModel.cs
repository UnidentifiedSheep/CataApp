using CatalogueAvalonia.Core;
using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel
{
	public partial class EditCatalogueViewModel : ViewModelBase
	{
		private readonly int? _uniId;
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		private readonly ObservableCollection<ProducerModel> _producers;
		public IEnumerable<ProducerModel> Producers => _producers;

		private readonly ObservableCollection<CatalogueModel> _catalogueModels;
		public IEnumerable<CatalogueModel> Catalogues => _catalogueModels;
		public bool IsDirty = false;

		[ObservableProperty]
		private string _nameOfPart = string.Empty;
		[ObservableProperty]
		private ProducerModel? _selectedProducer;
		[ObservableProperty]
		private CatalogueModel? _selectedCatalogue;
		
		public EditCatalogueViewModel()
		{
			_producers = new ObservableCollection<ProducerModel>();
			for (int i = 0; i < 10; i++)
				_producers.Add(new ProducerModel { Id = i, ProducerName = $"Producer{i}" });
			_catalogueModels = new ObservableCollection<CatalogueModel>();
			for (int i = 0; i < 10; i++)
				_catalogueModels.Add(new CatalogueModel { Name = "", UniValue=$"part{i}", ProducerName=$"sampa{i}"});
		}
		public EditCatalogueViewModel(IMessenger messenger, DataStore dataStore, int? id, TopModel topModel) : base(messenger)
		{
			_topModel = topModel;
			_uniId = id;
			_dataStore = dataStore;
			_producers = new ObservableCollection<ProducerModel>(_dataStore.ProducerModels);
			_catalogueModels = new ObservableCollection<CatalogueModel>();
			GetParts(_uniId);
		}

		private void GetParts(int? id)
		{
			NameOfPart = _dataStore.CatalogueModels.Where(x => x.UniId == id).OrderBy(x => x.UniId).First().Name;
			IsDirty = false;
			var model = _dataStore.CatalogueModels.Where(x => x.UniId == id).OrderBy(x => x.UniId).First().Children?.Select(x => new CatalogueModel
			{
				MainCatId = x.MainCatId,
				ProducerId = x.ProducerId,
				ProducerName = x.ProducerName,
				UniValue = x.UniValue,
				UniId = x.UniId,
				Name = x.Name,
			});
			if (model != null)
				_catalogueModels.AddRange(model);
        }
		partial void OnNameOfPartChanged(string value)
		{
			IsDirty = true;
		}
		[RelayCommand]
		private void AddNewPart()
		{
			_catalogueModels.Add(new CatalogueModel { MainCatId = null, UniId = _uniId, ProducerId = 0, ProducerName = "Неизвестный", Name = "Название не указано" });
			IsDirty = true;
		}
		private bool canDelete() => _catalogueModels.Any();
		[RelayCommand(CanExecute = nameof(canDelete))]
		private void RemovePart() 
		{
			if (SelectedCatalogue != null)
			{
				_catalogueModels.Remove(SelectedCatalogue);
				IsDirty = true;
			}
		}
		[RelayCommand]
		private async Task SaveChanges()
		{
			_catalogueModels.Remove(_catalogueModels.Where(x => string.IsNullOrEmpty(x.UniValue)).ToList());

			CatalogueModel model = new CatalogueModel
			{
				UniId = _uniId,
				Name = NameOfPart,
				Children = new(_catalogueModels)
			};
			await _topModel.EditCatalogueAsync(model);
			var what = await _topModel.GetCatalogueByIdAsync(_uniId ?? 5923);
			Messenger.Send(new EditedMessage(new ChangedItem { Where = "PartCatalogue", Id = _uniId, What = what}));
			_catalogueModels.Clear();
		}
		[RelayCommand]
		private async Task DeleteGroup()
		{
			await _topModel.DeleteGroupFromCatalogue(_uniId);
			Messenger.Send(new DeletedMessage(new DeletedItem { Where = "PartCatalogue", Id = _uniId }));
			_catalogueModels.Clear();
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
				IsDirty = true;
			}
		}
	}
}
