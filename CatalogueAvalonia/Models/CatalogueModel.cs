using CatalogueAvalonia.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class CatalogueModel
	{
		public int UniId { get; set; }
		public int PriceId { get; set; }
		public int MainCatId { get; set; }
		public int ProducerId { get; set; }
		public int CurrencyId { get; set; }

		public string UniValue { get; set; } = string.Empty;
		private string _name = string.Empty;
		public string Name { 
			get
			{
				return _name;
			}
			set
			{
				if (string.IsNullOrEmpty(value) || value == " ")
					_name = "Название не указано";
				else
					_name = value;
			}
		}
		public string ProducerName { get; set; } = string.Empty;
		public string CurrencyName { get; set; } = string.Empty;
		public int Count { get; set; }
		public double Price { get; set; }

		public ObservableCollection<CatalogueModel>? Children { get; set; }
	}
}
