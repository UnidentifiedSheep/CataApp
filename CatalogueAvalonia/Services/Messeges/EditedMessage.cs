using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges
{
	public class EditedMessage : ValueChangedMessage<ChangedItem>
	{
		public EditedMessage(ChangedItem value) : base(value)
		{
		}
	}
	public class ChangedItem
	{
		public string Where = string.Empty;
		public string MainName = string.Empty;
		public int? Id;
		public object? What;
	}
}
