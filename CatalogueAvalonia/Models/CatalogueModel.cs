using CatalogueAvalonia.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public partial class CatalogueModel : ObservableObject
	{
		public int? UniId { get; set; }
		public int PriceId { get; set; }
		public int? MainCatId { get; set; }
		public int ProducerId { get; set; }
		public int CurrencyId { get; set; }

		[ObservableProperty]
		private string _uniValue = string.Empty;
		[ObservableProperty]
		private string _producerName  = string.Empty;
		[ObservableProperty]
		private string _currencyName = string.Empty;
		[ObservableProperty]
		private int _count;
		[ObservableProperty]
		private double _price;
		[ObservableProperty]
		private string _name = " ";

		public ObservableCollection<CatalogueModel>? Children { get; set; }

		partial void OnNameChanged(string value)
		{
			if (string.IsNullOrEmpty(value) || value == " ")
				Name = "Название не указано";
		}
	}
}
