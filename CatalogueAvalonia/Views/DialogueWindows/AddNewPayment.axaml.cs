using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using CatalogueAvalonia.ViewModels.DialogueViewModel;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class AddNewPayment : Window
	{
		public AddNewPayment()
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
						if (!dc.ConvertFromCurr)
						{
							Close();
							dc.AddTransactionCommand.Execute(null);
						}
						else
						{
							if (dc.SelectedConvertCurrency != null)
							{
								Close();
								dc.AddTransactionCommand.Execute(null);
							}
							else
								await MessageBoxManager.GetMessageBoxStandard("?",
									$"Вы не выбрали валюту для конвертации.", ButtonEnum.Ok).ShowWindowDialogAsync(this);
						}
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
		public void PayAll_Click(object sender, RoutedEventArgs e)
		{
			var dc = (AddNewTransactionViewModel?)DataContext;
			if (dc != null) 
			{
				if (dc.TransactionData != null)
				{
					dc.TransactionSum = dc.TransactionData.TransactionSum;
					dc.AddNewTransactionNormalCommand.Execute(dc.TransactionSum * -1);
					Close();
				}
			}
			
		}
	}
}
