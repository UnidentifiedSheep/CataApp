using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public partial class ProdajaAltModel : ObservableObject
	{
		public int? Id { get; set; }
		public int ProdajaId { get; set; }
		public int? MainCatId { get; set; }
		[ObservableProperty]
		public int _maxCount;
		[ObservableProperty]
		public string? _mainCatName = string.Empty;
		[ObservableProperty]
		public string? _uniValue = string.Empty;
		[ObservableProperty]
		public string? _mainName = string.Empty;

		[ObservableProperty]
		public double _price;
		[ObservableProperty]
		public int _count;
		[ObservableProperty]
		private double _priceSum;

		partial void OnPriceChanged(double value)
		{
			PriceSum = Math.Round(Math.Round(Price, 2) * Count, 2);
		}
		partial void OnCountChanged(int value)
		{
			PriceSum = Math.Round(Math.Round(Price, 2) * Count, 2);
		}
	}
}
