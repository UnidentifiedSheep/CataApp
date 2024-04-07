﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CatalogueAvalonia.Models
{
	public partial class ZakupkaAltModel : ObservableObject
	{
		public int? Id { get; set; }
		public int ZakupkaId { get; set; }
		public int? MainCatId { get; set; }
		[ObservableProperty]
		public string? _mainCatName = string.Empty;
		[ObservableProperty]
		public string? _uniValue  = string.Empty;
		[ObservableProperty]
		public string? _mainName  = string.Empty;

		[ObservableProperty]
		public double _price;
		[ObservableProperty]
		public int _count;
		[ObservableProperty]
		private double _priceSum;
		[ObservableProperty] 
		private int _minCount = 0;
		[ObservableProperty] 
		private bool _canDelete = true;

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
