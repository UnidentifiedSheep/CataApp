using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges;

public class DeletedMessage : ValueChangedMessage<DeletedItem>
{
    public DeletedMessage(DeletedItem value) : base(value)
    {
    }
}

public class DeletedItem
{
    public int? Id;
    public int? SecondId;
    public string Where = string.Empty;
}