using System;
using Avalonia.Controls;
using MsBox.Avalonia;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DialogueServices
{
	public class DialogueService : IDialogueService
	{
		public async Task OpenDialogue(Window window, ViewModelBase viewModel, Window parent)
		{
			try
			{
				window.DataContext = viewModel;
				await window.ShowDialog(parent);
			}
			catch (Exception e)
			{
				await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Что то пошло не так!{e}").ShowAsync();
			}
		}

		public async void OpenWindow(Window window, ViewModelBase viewModel)
		{
			try
			{
				window.DataContext = viewModel;
				window.Show();
			}
			catch (Exception e)
			{
				await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Что то пошло не так!{e}").ShowAsync();
			}
		}
	}
}
