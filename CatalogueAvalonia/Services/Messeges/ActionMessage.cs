using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CatalogueAvalonia.Services.Messeges
{
	public class ActionMessage : ValueChangedMessage<string>
	{
		public ActionMessage(string value) : base(value)
		{
		}
	}
}
