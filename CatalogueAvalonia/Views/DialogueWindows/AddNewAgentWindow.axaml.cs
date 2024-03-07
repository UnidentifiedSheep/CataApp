using Avalonia.Controls;
using Avalonia.Interactivity;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class AddNewAgentWindow : Window
	{
		public AddNewAgentWindow()
		{
			InitializeComponent();
		}
		public void Cancle_ButtClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
		public async void Save_ButtClicked(object sender, RoutedEventArgs e)
		{
			var dc = (AddNewAgentViewModel?)DataContext;
			if (dc != null)
			{
				if (!string.IsNullOrEmpty(dc.AgentName))
				{
					dc.AddNewAgentCommand.Execute(null);
					Close();
				}
				else
					await MessageBoxManager.GetMessageBoxStandard("?",
						$"�� �� ����� ��� �����������", ButtonEnum.Ok).ShowWindowDialogAsync(this);
			}
		}
	}
}