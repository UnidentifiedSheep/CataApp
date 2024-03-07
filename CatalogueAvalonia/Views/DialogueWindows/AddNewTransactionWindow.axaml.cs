using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class AddNewTransactionWindow : Window
	{
		public AddNewTransactionWindow()
		{
			InitializeComponent();
		}
		public async void SaveButt_Click(object sender, RoutedEventArgs e)
		{
			var dc = (AddNewTransactionViewModel?)DataContext;
			if (dc != null) 
			{
				if (dc.TransactionSum != 0)
				{
					if (dc.SelectedCurrency != null)
					{
						dc.AddTransactionCommand.Execute(null);
						Close();
					}
					else
						await MessageBoxManager.GetMessageBoxStandard("?",
						$"Вы не выбрали валюту.", ButtonEnum.Ok).ShowWindowDialogAsync(this);
				}
				else
					await MessageBoxManager.GetMessageBoxStandard("?",
						$"Сумма равна 0.", ButtonEnum.Ok).ShowWindowDialogAsync(this);
			}
		}
		public void CancleButt_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
