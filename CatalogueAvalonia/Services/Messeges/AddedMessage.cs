using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges
{
	public class AddedMessage : ValueChangedMessage<ChangedItem>
	{
		public AddedMessage(ChangedItem value) : base(value)
		{
		}
	}
}
