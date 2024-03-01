using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DialogueServices
{
	public class DialogueService : IDialogueService
	{
		public async Task OpenDialogue(Window window, ViewModelBase viewModel, Window parent)
		{
			if (window != null && viewModel != null)
			{
				window.DataContext = viewModel;
				await window.ShowDialog(parent);

			}
			else
				await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Что то пошло не так!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
		}

		public async void OpenWindow(Window window, ViewModelBase viewModel)
		{
			if (window != null && viewModel != null)
			{
				window.DataContext = viewModel;
				window.Show();

			}
			else
				await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Что то пошло не так!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
		}
	}
}
