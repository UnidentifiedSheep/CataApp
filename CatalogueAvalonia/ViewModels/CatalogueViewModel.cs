﻿using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
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
		[ObservableProperty]
		private bool _isLoaded = !false;

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
					new TextColumn<CatalogueModel, double>(
						"Цена", x => x.Price, GridLength.Star),
					new TextColumn<CatalogueModel, int>(
						"Количество", x=> x.Count, GridLength.Star),
				}
			};
			Messenger.Register<ActionMessage>(this, OnAction);
			Messenger.Register<EditedMessage>(this, OnEditedIdDataBase);
			Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
			Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
		}

		private void OnAction(object recipient, ActionMessage message)
		{
			if (message.Value == "DataBaseLoaded")
			{
				Dispatcher.UIThread.Post(() =>
				{
					IsLoaded = !true;
					_catalogueModels.AddRange(_dataStore.CatalogueModels); 
				});
			}
		}

		private void OnDataBaseAdded(object recipient, AddedMessage message)
		{
			if (message.Value.Where == "Catalogue")
			{
				var what = message.Value.What as CatalogueModel;
				if (what != null)
				{
					_catalogueModels.Add(what);
				}
			}
		}

		private void OnDataBaseDeleted(object recipient, DeletedMessage message)
		{
			var where = message.Value.Where;
			if (where == "PartCatalogue")
			{
				_catalogueModels.Remove(_catalogueModels.Where(x => x.UniId == message.Value.Id).Single());
			}

		}

		private void OnEditedIdDataBase(object recipient, EditedMessage message)
		{
			var where = message.Value.Where;
			if (where == "PartCatalogue")
			{
				var uniId = message.Value.Id;
				var model = (CatalogueModel?)message.Value.What;
				if (model != null)
				{
					var item = _catalogueModels.Where(x => x.UniId == uniId).SingleOrDefault();
					if (item != null)
					{
						_catalogueModels.ReplaceOrAdd(item, model);
					}
				}
			}
		}
		private bool _isFilteringByUniValue = false;
		partial void OnPartNameChanged(string value)
		{
			var filter = new AsyncRelayCommand(async (CancellationToken token) =>
			{
				_catalogueModels.Clear();
				await foreach (var res in DataFiltering.FilterByMainName(_dataStore.CatalogueModels, value, token))
					_catalogueModels.Add(res);
			});
			if (_isFilteringByUniValue)
			{

			}
			else if (value.Length >= 3)
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
			if (value.Length >= 2)
			{
				filter.Execute(null);
				_isFilteringByUniValue = true;
			}
			else if (value.Length <= 1 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
			{
				_isFilteringByUniValue = false;
				_catalogueModels.Clear();
				_catalogueModels.AddRange(_dataStore.CatalogueModels);
				OnPartNameChanged(PartName);
			}
		}
		private bool CanDeleteGroup()
		{
			_selecteditem = CatalogueModels.RowSelection?.SelectedItem;
			return _selecteditem != null && _selecteditem.UniId != null && _selecteditem.MainCatId == null && _selecteditem.UniId != 5923;
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
			_selecteditem = CatalogueModels.RowSelection?.SelectedItem;
			return _selecteditem != null && _selecteditem.MainCatId != null && _selecteditem.UniId != null && _selecteditem.UniId != 5923;
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
					var model = _catalogueModels.Where(x => x.UniId == _selecteditem.UniId).SingleOrDefault();

                    if (model != null)
                    {
						await _topModel.DeleteSoloFromCatalogue(_selecteditem.MainCatId);
						var item = _dataStore.CatalogueModels.Where(x => x.UniId == _selecteditem.UniId).SingleOrDefault();
						if (item != null && item.Children != null) 
						{ 
							item.Children.Remove(_selecteditem);
						}
					}
				}
			}

		}
		private bool CanEditPrices() 
		{
			_selecteditem = CatalogueModels.RowSelection?.SelectedItem;
			return _selecteditem != null && _selecteditem.MainCatId != null && _selecteditem.UniId != 5923;
		} 
		[RelayCommand(CanExecute = nameof(CanEditPrices))]
		private async Task EditPrices(Window parent)
		{
			if (_selecteditem != null) 
			{
				await _dialogueService.OpenDialogue(new EditPricesWindow(), new EditPricesViewModel(Messenger, _topModel, _selecteditem.MainCatId ?? default, _dataStore, _selecteditem.UniValue), parent);
			}
		}
		private bool canEditCatalogue()
		{
			_selecteditem = CatalogueModels.RowSelection?.SelectedItem;
			return _selecteditem != null && _selecteditem.UniId != null && _selecteditem.UniId != 5923;
		}

		[RelayCommand(CanExecute = nameof(canEditCatalogue))]
		private async Task EditCatalogue(Window parent)
		{
			if (_selecteditem != null)
			{
				await _dialogueService.OpenDialogue(new EditCatalogueWindow(), new EditCatalogueViewModel(Messenger, _dataStore, _selecteditem.UniId, _topModel), parent);
			}
		}
		[RelayCommand]
		private async Task AddNewPart(Window parent)
		{
			await _dialogueService.OpenDialogue(new AddNewPartView(), new AddNewPartViewModel(Messenger, _dataStore, _topModel), parent);
		}
	}

}
