using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using System.Linq;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class EditCatalogueWindow : Window
	{
		public EditCatalogueWindow()
		{
			InitializeComponent();
		}
		public void DataGrid_CellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
		{
			var dc = (EditCatalogueViewModel?)DataContext;

			if (dc != null) 
			{ 
				dc.IsDirty = true;
			}
		}
		public async void SaveButt_Click(object sender, RoutedEventArgs e)
		{
			var dc = (EditCatalogueViewModel?)DataContext;
			if (dc != null) 
			{
				if (dc.IsDirty)
				{
					if (dc.Catalogues.Any())
					{
						Close();
						dc.SaveChangesCommand.Execute(null);
					}
					else 
					{
						var res = await MessageBoxManager.GetMessageBoxStandard
								(
									"Список запчастей пуст", 
								    "Вы уверены что хотите удалить группу запчастей?", 
								    MsBox.Avalonia.Enums.ButtonEnum.YesNo
								).ShowWindowDialogAsync(this);
						if (res == MsBox.Avalonia.Enums.ButtonResult.Yes)
						{
							Close();
							dc.DeleteGroupCommand.Execute(null);
						}
						else
						{
							Close();
						}
					}
				}
				else
					Close();
			}
		}
		public void CancleButt_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
