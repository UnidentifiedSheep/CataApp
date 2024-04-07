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
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels
{
	public partial class ZakupkaViewModel : ViewModelBase
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
		private ZakupkiModel? _selectedZakupki;
		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(NewZakupkaCommand))]
		private bool _isLoaded = false;
		private readonly ObservableCollection<ZakupkiModel> _zakupkiMainGroup;
		public IEnumerable<ZakupkiModel> ZakupkiMainGroup => _zakupkiMainGroup;
		private readonly ObservableCollection<AgentModel> _agents;
		public IEnumerable<AgentModel> Agents => _agents;
		private readonly ObservableCollection<ZakupkaAltModel> _altGroup;
		public IEnumerable<ZakupkaAltModel> AltGroup => _altGroup;

		public ZakupkaViewModel() 
		{
			_startDate = DateTime.Now.AddMonths(-1).Date;
			_endDate = DateTime.Now.Date;
			_agents = new ObservableCollection<AgentModel>();
			_zakupkiMainGroup = new ObservableCollection<ZakupkiModel>();
			_altGroup = new ObservableCollection<ZakupkaAltModel>();
		}
		public ZakupkaViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService) : base(messenger)
		{
			_dataStore = dataStore;
			_topModel = topModel;
			_dialogueService = dialogueService;

			_startDate = DateTime.Now.AddMonths(-1).Date;
			_endDate = DateTime.Now.Date;
			_agents = new ObservableCollection<AgentModel>();
			_zakupkiMainGroup = new ObservableCollection<ZakupkiModel>();
			_altGroup = new ObservableCollection<ZakupkaAltModel>();

			Messenger.Register<ActionMessage>(this, OnDataBaseAction);
			Messenger.Register<EditedMessage>(this, OnDataBaseEdited);
		}

		private void OnDataBaseEdited(object recipient, EditedMessage message)
		{
			string where = message.Value.Where;
			int? id = message.Value.Id;
			if (where == "AgentEdited") 
			{
				if (id != null) 
				{
					var agent = _dataStore.AgentModels.SingleOrDefault(x => x.Id == id);
					if (agent != null)
					{
						var inModel = _agents.FirstOrDefault(x => x.Id == agent.Id);
						if (Convert.ToBoolean(agent.IsZak))
						{ 
							Dispatcher.UIThread.Post(() =>
							{
								if (inModel != null) 
									_agents.ReplaceOrAdd(inModel, agent);
								else
									_agents.Add(agent);
							});
						}
						else
						{
							Dispatcher.UIThread.Post(() =>
							{
								if (inModel != null)
									_agents.Remove(inModel);
							});
						}
						
					}
				}
			}
		}

		private void OnDataBaseAction(object recipient, ActionMessage message)
		{
			if (message.Value == "DataBaseLoaded")
			{
				Dispatcher.UIThread.Post(() =>
				{
					_agents.AddRange(_dataStore.AgentModels.Where(x => x.IsZak == 1));
					IsLoaded = true;
				});
			}
			else if (message.Value == "Update")
			{
				Dispatcher.UIThread.Post(() =>
				{
					LoadZakupkiMainGroupCommand.Execute(null);
					LoadZakupkiAltGroupCommand.Execute(null);
				});
			}
		}
		partial void OnEndDateChanged(DateTime value)
		{
			LoadZakupkiMainGroupCommand.Execute(null);
		}
		partial void OnStartDateChanged(DateTime value)
		{
			LoadZakupkiMainGroupCommand.Execute(null);
		}
		partial void OnSelectedAgentChanged(AgentModel? value)
		{
			LoadZakupkiMainGroupCommand.Execute(null);
		}
		partial void OnSelectedZakupkiChanged(ZakupkiModel? value)
		{
			LoadZakupkiAltGroupCommand.Execute(null);
		}
		[RelayCommand]
		private async Task LoadZakupkiMainGroup()
		{
			if (SelectedAgent != null)
			{
				_zakupkiMainGroup.Clear();
				string startDate = Converters.ToDateTimeSqlite(StartDate.Date.ToString("dd.MM.yyyy"));
				string endDate = Converters.ToDateTimeSqlite(EndDate.Date.ToString("dd.MM.yyyy"));
				_zakupkiMainGroup.AddRange(await _topModel.GetZakupkiMainGroupAsync(startDate, endDate, SelectedAgent.Id));
			}
		}
		[RelayCommand]
		private async Task LoadZakupkiAltGroup()
		{
			_altGroup.Clear();
			if (SelectedZakupki != null)
			{
				_altGroup.AddRange(await _topModel.GetZakAltGroup(SelectedZakupki.Id));
			}
		}
		[RelayCommand(CanExecute = nameof(IsLoaded))]
		private async Task NewZakupka(Window parent)
		{
			await _dialogueService.OpenDialogue(new NewPurchaseWindow("NewPurchaseViewModel"), new NewPurchaseViewModel(Messenger, _dataStore, _topModel, _dialogueService), parent);
		}
		[RelayCommand]
		private async Task DeleteZakupkaMainGroup(Window parent)
		{
			if (SelectedZakupki != null)
			{
				bool canDelete = false;
				var tfList = new List<bool>();
				ZakupkaAltModel? model = null;
				foreach (var item in _altGroup)
				{
					int? diff = await _topModel.CanDeleteProdaja(item.MainCatId);
					if (diff != null)
					{
						int itog = (diff ?? 0) - item.Count;
						if (itog >= 0)
							tfList.Add(true);
						else
						{
							tfList.Add(false);
							model = item;
							break;
						}
					}
				}
				if (tfList.All(x => x ))
					canDelete = true;
				

				if (canDelete)
				{
					var res = await MessageBoxManager.GetMessageBoxStandard("Удалить закупку?",
						$"Вы уверенны что хотите удалить закупку с номером \"{SelectedZakupki.Id}\"?",
						ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
					if (res == ButtonResult.Yes)
					{
						var catas = await _topModel.DeleteZakupkaWithPricesReCount(SelectedZakupki.TransactionId, _altGroup);
						Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
						Messenger.Send(new ActionMessage("Update"));
					}
				}
				else
				{
					if (model != null)
						await MessageBoxManager.GetMessageBoxStandard("Нельзя удалить закупку",
							$"Нельзя удалить закупку т.к зачасть с номером \"{model.UniValue}\" " +
							$"и названием \"{model.MainName}\", продана в большем кол-ве чем присутствует на данный момент на складе." +
							$"\n Для начала удалите или отредактируйте продажу с данным номером/ами.").ShowWindowDialogAsync(parent);
					else
						await MessageBoxManager.GetMessageBoxStandard("Нельзя удалить закупку",
							$"Нельзя удалить закупку т.к какая-то запчасть, продана в большем кол-ве чем присутствует на данный момент на складе. " +
							$"\n Для начала удалите или отредактируйте продажу с данным номером/ами.").ShowWindowDialogAsync(parent);
				}
			}
		}
		[RelayCommand]
		private async Task EditZakupka(Window parent)
		{
			if (SelectedZakupki != null)
			{
				await _dialogueService.OpenDialogue(new NewPurchaseWindow("EditPurchaseViewModel"), new EditPurchaseViewModel(Messenger, _dataStore, _topModel, _dialogueService, SelectedZakupki), parent);
			}
		}
	}
}
