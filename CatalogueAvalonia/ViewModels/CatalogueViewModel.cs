using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Models.TreeDataGrid;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
		private readonly IDialogueService _dialogueService;

		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		private CatalogueModel? _selecteditem;
		private readonly ObservableCollection<CatalogueModel> _catalogueModels;
		public HierarchicalTreeDataGridSource<CatalogueModel> CatalogueModels { get; }

		[ObservableProperty]
		private string _partName = string.Empty;
		[ObservableProperty]
		private string _partUniValue = string.Empty;
		
		public CatalogueViewModel() { }
		public CatalogueViewModel(IMessenger messenger, DataStore dataStore, 
								  TopModel topModel, IDialogueService dialogueService) : base(messenger)
		{
			_dialogueService = dialogueService;
			_topModel = topModel;
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
			Messenger.Register<EditedMessage>(this, OnEditedIdDataBase);
			Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
		}

		private void OnDataBaseDeleted(object recipient, DeletedMessage message)
		{
			if (message.Value.Where == "PartCatalogue")
			{
				_catalogueModels.Remove(_catalogueModels.Where(x => x.UniId == message.Value.Id).Single());
			}
		}

		private void OnEditedIdDataBase(object recipient, EditedMessage message)
		{
			if (message.Value.Where == "PartCatalogue")
			{
				var uniId = message.Value.Id;
				var model = (CatalogueModel?)message.Value.What;
				if (model != null)
				{
					_catalogueModels.Remove(_catalogueModels.Where(x => x.UniId == model.UniId).Single());
					_catalogueModels.Add(model);
				}
			}
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
		private bool CanDeleteGroup()
		{
			_selecteditem = CatalogueModels.RowSelection?.SelectedItem;
			if(_selecteditem != null) 
			{
				if (_selecteditem.UniId != null && _selecteditem.MainCatId == null)
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		[RelayCommand(CanExecute = nameof(CanDeleteGroup))]
		private async Task DeleteGroup(Window parent)
		{
			if (_selecteditem != null) 
			{
				var res = await MessageBoxManager.GetMessageBoxStandard("Удалить группу запчастей", 
						$"Вы уверенны что хотите удалить: \n\"{_selecteditem.Name}\"?", 
						ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
				if (res == ButtonResult.Yes)
				{
					_catalogueModels.Remove(_selecteditem);
					_dataStore.CatalogueModels.Remove(_selecteditem);
					await _topModel.DeleteGroupFromCatalogue(_selecteditem.UniId);
				}
			}
				
		}
		private bool CanDeletePart()
		{
			if (_selecteditem != null) 
			{
				if (_selecteditem.MainCatId != null)
				{
					return true;
				}
				else
					return false;
			}
			return false;
		}
		[RelayCommand(CanExecute = nameof(CanDeletePart))]
		private async Task DeleteSolo(Window parent)
		{
			if (_selecteditem != null)
			{
				var res = await MessageBoxManager.GetMessageBoxStandard("Удалить запчасть",
						$"Вы уверенны что хотите удалить: \n\"{_selecteditem.UniValue}\"?",
						ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
				if (res == ButtonResult.Yes) 
				{
					_catalogueModels.Remove(_selecteditem);
					_dataStore.CatalogueModels.Remove(_selecteditem);
					await _topModel.DeleteSoloFromCatalogue(_selecteditem.MainCatId);
				}
			}
			
		}
		[RelayCommand]
		private async Task EditCatalogue(Window parent)
		{
			if (_selecteditem != null)
			{
				if (_selecteditem.MainCatId == null && _selecteditem.UniId == null)
				{
					
				}
				else
				{
					await _dialogueService.OpenDialogue(new EditCatalogueWindow(), new EditCatalogueViewModel(Messenger, _dataStore, _selecteditem.UniId, _topModel), parent);
				}
			}
		}
	}

}
