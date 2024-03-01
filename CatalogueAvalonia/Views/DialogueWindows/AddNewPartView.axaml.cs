using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Linq;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class AddNewPartView : Window
	{
		public AddNewPartView()
		{
			InitializeComponent();
		}
		public async void SaveButt_Click(object sender, RoutedEventArgs e)
		{
			var dc = (AddNewPartViewModel?)DataContext;
			if (dc != null) 
			{
				if (dc.Catalogues.Any())
				{
					if(!string.IsNullOrEmpty(dc.NameOfParts))
					{
						dc.AddToCatalogueCommand.Execute(null);
						Close();
					}
					else
						await MessageBoxManager.GetMessageBoxStandard("?",
						$"Вы не ввели название группы запчастей.", ButtonEnum.Ok).ShowWindowDialogAsync(this);
				}
				else
				{
					await MessageBoxManager.GetMessageBoxStandard("?",
						$"Вы ввели ни одной запчасти.", ButtonEnum.Ok).ShowWindowDialogAsync(this);
					Close();
				}
			}
		}
		public void CancleButt_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
