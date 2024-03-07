using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
