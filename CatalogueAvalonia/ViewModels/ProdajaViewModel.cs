using Avalonia.Controls;
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
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CatalogueAvalonia.Services.BillingService;

namespace CatalogueAvalonia.ViewModels
{
	public partial class ProdajaViewModel : ViewModelBase
	{
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		private readonly IDialogueService _dialogueService;
		[ObservableProperty]
		private DateTime _startDate;
		[ObservableProperty]
		private DateTime _endDate;
		[ObservableProperty]
		private AgentModel? _selectedAgent;
		[ObservableProperty]
		private ProdajaModel? _selectedProdaja;
		[ObservableProperty]
		private ProdajaAltModel? _selectedAltModel;

		[ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NewProdajaCommand))]
		private bool _isLoaded;

		private readonly ObservableCollection<ProdajaModel> _prodajaMainGroup;
		public IEnumerable<ProdajaModel> MainGroup => _prodajaMainGroup;
		private readonly ObservableCollection<ProdajaAltModel> _prodajaAltGroup;
		public IEnumerable<ProdajaAltModel> AltGroup => _prodajaAltGroup;
		private readonly ObservableCollection<AgentModel> _agents;
		public IEnumerable<AgentModel> Agents => _agents;

		public ProdajaViewModel() 
		{
			_startDate = DateTime.Now.AddMonths(-1).Date;
			_endDate = DateTime.Now.Date;
			_prodajaAltGroup = new ObservableCollection<ProdajaAltModel>();
			_prodajaMainGroup = new ObservableCollection<ProdajaModel>();
			_agents = new ObservableCollection<AgentModel>();
		}
		public ProdajaViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
		{
			_isLoaded = false;
			_dataStore = dataStore;
			_topModel = topModel;
			_dialogueService = dialogueService;

			_startDate = DateTime.Now.AddMonths(-1).Date;
			_endDate = DateTime.Now.Date;

			_prodajaAltGroup = new ObservableCollection<ProdajaAltModel>();
			_prodajaMainGroup = new ObservableCollection<ProdajaModel>();
			_agents = new ObservableCollection<AgentModel>();

			Messenger.Register<ActionMessage>(this, OnDataBaseAction);
		}

		private void OnDataBaseAction(object recipient, ActionMessage message)
		{
			if (message.Value == "DataBaseLoaded")
			{
				Dispatcher.UIThread.Post(() =>
				{
					_agents.AddRange(_dataStore.AgentModels);
					IsLoaded = true;
				});
			}
			else if (message.Value == "Update")
			{
				Dispatcher.UIThread.Post(() =>
				{
					LoadProdajaMainGroupCommand.Execute(null);
					LoadProdajaAltGroupCommand.Execute(null);
				});
			}
		}
		[RelayCommand]
		private async Task LoadProdajaMainGroup()
		{
			if (SelectedAgent != null)
			{
				_prodajaMainGroup.Clear();
				string startDate = Converters.ToDateTimeSqlite(StartDate.Date.ToString("dd.MM.yyyy"));
				string endDate = Converters.ToDateTimeSqlite(EndDate.Date.ToString("dd.MM.yyyy"));
				_prodajaMainGroup.AddRange(await _topModel.GetProdajaMainGroupAsync(startDate, endDate, SelectedAgent.Id));
			}
		}
		[RelayCommand]
		private async Task LoadProdajaAltGroup()
		{
			_prodajaAltGroup.Clear();
			if (SelectedProdaja != null)
			{
				_prodajaAltGroup.AddRange(await _topModel.GetProdajaAltGroupAsync(SelectedProdaja.Id));
			}
		}
		partial void OnEndDateChanged(DateTime value)
		{
			LoadProdajaMainGroupCommand.Execute(null);
		}
		partial void OnStartDateChanged(DateTime value)
		{
			LoadProdajaMainGroupCommand.Execute(null);
		}
		partial void OnSelectedAgentChanged(AgentModel? value)
		{
			LoadProdajaMainGroupCommand.Execute(null);
		}
		partial void OnSelectedProdajaChanged(ProdajaModel? value)
		{
			LoadProdajaAltGroupCommand.Execute(null);
		}
		[RelayCommand]
		private async Task NewProdaja(Window parent)
		{
			await _dialogueService.OpenDialogue(new NewProdajaWindow("NewProdajaViewModel"), new NewProdajaViewModel(Messenger, _topModel, _dataStore, _dialogueService), parent);
		}
		[RelayCommand]
		private async Task DeleteProdaja(Window parent)
		{
			if (SelectedProdaja != null)
			{
				var res = await MessageBoxManager.GetMessageBoxStandard("Удалить закупку?",
						$"Вы уверенны что хотите удалить закупку с номером {SelectedProdaja.Id}?",
						ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
				if (res == ButtonResult.Yes)
				{
					IEnumerable<CatalogueModel> catas = new List<CatalogueModel>();
					catas = await _topModel.DeleteProdajaAsync(SelectedProdaja.TransactionId, _prodajaAltGroup, SelectedProdaja.CurrencyId);
					Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
					Messenger.Send(new ActionMessage("Update"));
				}
			}
		}

		[RelayCommand]
		private void CreateInvoice()
		{
			if (SelectedProdaja != null)
			{
				Invoice.CreateInvoice(_prodajaAltGroup, SelectedProdaja);
			}
		}
		[RelayCommand]
		private async Task EditProdaja(Window parent)
		{
			if (SelectedProdaja != null) 
			{
				await _dialogueService.OpenDialogue(new NewProdajaWindow("EditProdajaViewModel"), new EditProdajaViewModel(Messenger, _dataStore, _topModel, _dialogueService, SelectedProdaja), parent);
			}
		}
	}
}
