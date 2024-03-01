using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.Messeges
{
	public class AddedMessage : ValueChangedMessage<ChangedItem>
	{
		public AddedMessage(ChangedItem value) : base(value)
		{
		}
	}
}
