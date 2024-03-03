using CatalogueAvalonia.Services.DataStore;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace CatalogueAvalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
	{
		private readonly ObservableCollection<object> _viewModels;
		public MainWindowViewModel(IMessenger messenger, CatalogueViewModel catalogueViewModel, AgentViewModel agentViewModel, DataStore dataStore) : base(messenger) 
		{
			_viewModels =
				[
					catalogueViewModel,
					agentViewModel,
				];
			var load = new AsyncRelayCommand(dataStore.LoadLazy);
			load.Execute(null);
		}
		public ObservableCollection<object> ViewModels { get { return _viewModels; } }
	}
}
