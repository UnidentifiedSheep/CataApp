using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class CurrencyModel
	{
		public int? Id { get; set; } = null;
		public string CurrencyName { get; set; } = string.Empty;
		private double _toUsd;
		public double ToUsd 
		{ 
			get
			{
				return _toUsd;
			}
			set
			{
				_toUsd = value;
				IsDirty = true;
			}
		}
		public int CanDelete { get; set; }
		public string CurrencySign { get; set; } = string.Empty;


		public bool IsDirty = false;
	}
}
