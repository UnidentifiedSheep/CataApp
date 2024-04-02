using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Linq;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class EditPricesWindow : Window
	{
		public EditPricesWindow()
		{
			InitializeComponent();
		}
		public void DataGridCellChanged(object sender, DataGridCellEditEndedEventArgs e) 
		{
			var dc = (EditPricesViewModel?)DataContext;
			if (dc != null)
			{
				dc.IsDirty = true;
			}
		}
		public async void SaveButt_Click(object sender, RoutedEventArgs e)
		{
			var dc = (EditPricesViewModel?)DataContext;
			if (dc != null)
			{
				if (dc.IsDirty || dc.MainCatPrices.Any(x => x.IsDirty))
				{
					if (dc.MainCatPrices.Any())
					{
						Close();
						dc.SaveChangesCommand.Execute(null);
					}
					else
					{
						var res =await MessageBoxManager.GetMessageBoxStandard("?",$"¬ы уверены что хотите удалить все позиции?",
						ButtonEnum.YesNo).ShowWindowDialogAsync(this);

						if (res == ButtonResult.Yes)
						{
							dc.SaveChangesCommand.Execute(null);
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
