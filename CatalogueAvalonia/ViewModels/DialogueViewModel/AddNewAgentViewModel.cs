using System;
using System.Text;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewAgentViewModel : ViewModelBase
{
    private readonly TopModel _topModel;
    private readonly DataStore _dataStore;

    [ObservableProperty] private string _agentName = string.Empty;

    [ObservableProperty] private int _isZak;

    public AddNewAgentViewModel()
    {
    }

    public AddNewAgentViewModel(IMessenger messenger, TopModel topModel, DataStore dataStore) : base(messenger)
    {
        _topModel = topModel;
        _dataStore = dataStore;
    }

    [RelayCommand]
    private async Task AddNewAgent()
    {
        var agent = await _topModel.AddNewAgentAsync(AgentName, IsZak);
        foreach (var curr in _dataStore.CurrencyModels)
            await _topModel.GetLastTransactionAsync(agent.Id, curr.Id ?? default);
        
        Messenger.Send(new AddedMessage(new ChangedItem { What = agent, Where = "Agent", Id = agent.Id }));
        Messenger.Send(new ActionMessage("Update"));
    }
}