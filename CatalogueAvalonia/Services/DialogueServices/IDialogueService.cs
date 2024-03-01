using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DialogueServices
{
	public interface IDialogueService
	{
		Task OpenDialogue(Window window, ViewModelBase viewModel, Window parent);
		void OpenWindow(Window window, ViewModelBase viewModel);
	}
}
