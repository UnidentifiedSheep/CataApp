using System.Threading.Tasks;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class AddNewAgentViewModel : ViewModelBase
{
    private readonly TopModel _topModel;

    [ObservableProperty] private string _agentName = string.Empty;

    [ObservableProperty] private int _isZak;

    public AddNewAgentViewModel()
    {
    }

    public AddNewAgentViewModel(IMessenger messenger, TopModel topModel) : base(messenger)
    {
        _topModel = topModel;
    }

    [RelayCommand]
    private async Task AddNewAgent()
    {
        var agent = await _topModel.AddNewAgentAsync(AgentName, IsZak);
        Messenger.Send(new AddedMessage(new ChangedItem { What = agent, Where = "Agent", Id = agent.Id }));
    }
}