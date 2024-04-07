using System;
using System.Collections.ObjectModel;

namespace CatalogueAvalonia.Models
{
	public class MainCatPriceModel
	{
		public int? Id { get; set; }

		public int MainCatId { get; set; }

		public int CurrencyId { get; set; }
		private double _price;
		public double Price
		{
			get { return _price; }
			set 
			{ 
				_price = Math.Round(value, 2); 
				IsDirty = true;
			}
		}
		public bool IsDirty { get; set; } = false;
		private int _count;
		public int Count 
		{ 
			get
			{
				return _count;
			}
			set
			{
				_count = value;
				IsDirty = true;
			}
		}
		private CurrencyModel? _selectedCurrency;
		public CurrencyModel? SelectedCurrency 
		{ 
			get
			{
				return _selectedCurrency;
			}
			set
			{
				_selectedCurrency = value;
				if (value != null)
					CurrencyId = value.Id ?? default;
				IsDirty = true;
			}
		}
		public ObservableCollection<CurrencyModel>? Currency { get; set; } = null;
	}
}
