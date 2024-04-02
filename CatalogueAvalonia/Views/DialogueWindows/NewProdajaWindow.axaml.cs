using Avalonia.Controls;
using System.Linq;
using System;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Microsoft.CodeAnalysis.CSharp;

namespace CatalogueAvalonia.Views.DialogueWindows
{
	public partial class NewProdajaWindow : Window
	{
		public NewProdajaWindow()
		{
			InitializeComponent();
		}
		private void NumericUpDownPrice_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
		{
			var dc = (NewProdajaViewModel?)DataContext;

			if (dc != null)
			{
				if (e.NewValue != null)
				{ 
					if (dc.SelectedProdaja != null)
					{
						dc.SelectedProdaja.Price = (double)e.NewValue;
						dc.TotalSum = Math.Round(dc.Prodaja.Sum(x => x.PriceSum), 2);
					}
				}
				
			}
		}
		private void NumericUpDownCount_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
		{
			var dc = (NewProdajaViewModel?)DataContext;

			if (dc != null)
			{

				if (e.NewValue != null)
				{ 
					if (dc.SelectedProdaja != null)
					{ 
						dc.SelectedProdaja!.Count = (int)e.NewValue; 
						dc.TotalSum = Math.Round(dc.Prodaja.Sum(x => x.PriceSum), 2);
					}
				}

			}
		}
		private async void SaveButt_Clicked(object sender, RoutedEventArgs e)
		{
			var dc = (NewProdajaViewModel?)DataContext;
			if (dc != null) 
			{
				if (dc.Prodaja.Any())
				{
					var whereZero = dc.Prodaja.Where(x => x.Count <= 0 || x.Price <= 0);
					if (!whereZero.Any())
					{
						if (dc.SelectedAgent != null)
						{
							if (dc.SelectedCurrency != null)
							{
								dc.SaveChangesCommand.Execute(this);
							}
							else
								await MessageBoxManager.GetMessageBoxStandard("?",
								$"Вы не выбрали валюту.",
								ButtonEnum.Ok).ShowWindowDialogAsync(this);
						}
						else
							await MessageBoxManager.GetMessageBoxStandard("?",
							$"Вы не выбрали контрагента.",
							ButtonEnum.Ok).ShowWindowDialogAsync(this);
					}
					else
					{
						var res = await MessageBoxManager.GetMessageBoxStandard("?",
						$"Есть позиции у которых Цена либо Количество = 0.\n Удалить их и продолжить закупку?",
						ButtonEnum.YesNo).ShowWindowDialogAsync(this);
						if (res == ButtonResult.Yes)
							dc.RemoveWhereZero(whereZero);
					}
				}
				else
					await MessageBoxManager.GetMessageBoxStandard("?",
							$"Вы не добавили ни одной позиции.",
							ButtonEnum.Ok).ShowWindowDialogAsync(this);
			}
		}
		private void CancleButt_Clicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
