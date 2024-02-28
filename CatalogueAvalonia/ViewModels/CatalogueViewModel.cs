using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueAvalonia.ViewModels
{
	public partial class CatalogueViewModel : ViewModelBase
	{
		private CatalogueModel? _selecteditem;
		private readonly ObservableCollection<CatalogueModel> _catalogueModels;
		public HierarchicalTreeDataGridSource<CatalogueModel> CatalogueModels { get; }

		[ObservableProperty]
		private string _partName = string.Empty;
		[ObservableProperty]
		private string _partUniValue = string.Empty;
		
		
		
		public DataBaseProvider dataBaseProvider = new();





		public CatalogueViewModel() 
		{
			_catalogueModels = new ObservableCollection<CatalogueModel>();
			var a = new AsyncRelayCommand(async () =>
			{
				foreach (var model in await dataBaseProvider.GetCatalogueAsync())
					_catalogueModels.Add(model);
			});
			a.Execute(null);

			CatalogueModels = new HierarchicalTreeDataGridSource<CatalogueModel>(_catalogueModels)
			{
				Columns =
				{
					new HierarchicalExpanderColumn<CatalogueModel>(
						new TextColumn<CatalogueModel, string>
							("Название", x => x.Name, new GridLength(580)), x => x.Children),
					new TextColumn<CatalogueModel, string>(
						"Номер запчасти", x => x.UniValue, new GridLength(150)),
					new TextColumn<CatalogueModel, string>(
						"Производитель", x => x.ProducerName, new GridLength(130)),
					new TextColumn<CatalogueModel, int>(
						"Количество", x=> x.Count, GridLength.Star),
					new TextColumn<CatalogueModel, double>(
						"Цена", x => x.Price, GridLength.Star)
				}
			};
			
		}
		partial void OnPartNameChanged(string value)
		{
			
		}
		[RelayCommand]
		private async Task DeleteGroup()
		{

		}
		[RelayCommand]
		private async Task DeleteSolo()
		{

		}
		[RelayCommand]
		private async Task EditCatalogue()
		{

		}
	}

}
