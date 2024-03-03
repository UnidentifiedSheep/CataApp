using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class CurrencyModel
	{
		public int Id { get; set; }

		public string CurrencyName { get; set; } = string.Empty;

		public double ToUsd { get; set; }
		public string CurrencySign { get; set; } = string.Empty;
	}
}
