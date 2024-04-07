using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges
{
	public class DeletedMessage : ValueChangedMessage<DeletedItem>
	{
		public DeletedMessage(DeletedItem value) : base(value)
		{
		}
	}
	public class DeletedItem
	{
		public string Where = string.Empty;
		public int? Id;
		public int? SecondId;
	}
}
