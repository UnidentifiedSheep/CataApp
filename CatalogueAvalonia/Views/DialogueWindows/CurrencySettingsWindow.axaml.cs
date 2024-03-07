using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using System.Linq;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class CurrencySettingsWindow : Window
	{
		public CurrencySettingsWindow()
		{
			InitializeComponent();
		}
		public void EditEnded(object sender, DataGridCellEditEndedEventArgs e)
		{
			var dc = (CurrencySettingsViewModel?)DataContext;
			if (dc != null ) 
			{
				dc.IsDirty = true;
			}
		}
		public void SaveButt_Click(object sender, RoutedEventArgs e)
		{
			var dc = (CurrencySettingsViewModel?)DataContext;
			if (dc != null) 
			{ 
				if (dc.IsDirty || dc.CurrencyModels.Where(x => x.IsDirty).Any()) 
				{
					dc.SaveChangesCommand.Execute(this);
					Close();
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
