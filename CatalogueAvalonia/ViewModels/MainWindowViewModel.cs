using Avalonia.Controls;
using Avalonia.Threading;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
	{
		private readonly IDialogueService _dialogueService;
		private readonly DataStore _dataStore;
		private readonly TopModel _topModel;
		private readonly ObservableCollection<object> _viewModels;
		private bool _isDataBaseLoaded = false;
		public MainWindowViewModel(IMessenger messenger, CatalogueViewModel catalogueViewModel, 
			AgentViewModel agentViewModel, DataStore dataStore, 
			IDialogueService dialogueService, TopModel topModel) : base(messenger) 
		{
			_dataStore = dataStore;
			_topModel = topModel;
			_dialogueService = dialogueService;
			_viewModels =
				[
					catalogueViewModel,
					agentViewModel,
				];
			Messenger.Register<ActionMessage>(this, OnDataBaseLoaded);
			
		}

		private void OnDataBaseLoaded(object recipient, ActionMessage message)
		{
			if (message.Value == "DataBaseLoaded")
			{
				Dispatcher.UIThread.Post(() => _isDataBaseLoaded = true);
			}
		}
		private bool IsLoaded() => _isDataBaseLoaded;
		public ObservableCollection<object> ViewModels { get { return _viewModels; } }
		[RelayCommand(CanExecute = nameof(IsLoaded))]
		private async Task OpenCurrencySettings(Window parent)
		{
			await _dialogueService.OpenDialogue(new CurrencySettingsWindow(), new CurrencySettingsViewModel(Messenger, _dataStore, _topModel), parent);
		}
	}
	
}
