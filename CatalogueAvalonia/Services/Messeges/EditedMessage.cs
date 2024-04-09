using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges;

public class EditedMessage : ValueChangedMessage<ChangedItem>
{
    public EditedMessage(ChangedItem value) : base(value)
    {
    }
}

public class ChangedItem
{
    public int? Id;
    public string MainName = string.Empty;
    public object? What;
    public string Where = string.Empty;
}