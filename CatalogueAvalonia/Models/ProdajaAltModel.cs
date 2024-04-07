using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CatalogueAvalonia.Models
{
	public partial class ProdajaAltModel : ObservableObject
	{
		public int? Id { get; set; }
		public int ProdajaId { get; set; }
		public int? MainCatId { get; set; }
		[ObservableProperty]
		private int _maxCount;
		[ObservableProperty]
		private string? _mainCatName = string.Empty;
		[ObservableProperty]
		private string? _uniValue = string.Empty;
		[ObservableProperty]
		private string? _mainName = string.Empty;
		[ObservableProperty]
		private string _producerName = string.Empty;
		[ObservableProperty]
		private double _price;
		[ObservableProperty]
		private int _count;
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
