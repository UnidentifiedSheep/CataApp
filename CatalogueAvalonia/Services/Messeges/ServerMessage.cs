using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges;

public class ServerMessage : ValueChangedMessage<string>
{
    public ServerMessage(string value) : base(value)
    {
    }
}