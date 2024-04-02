using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class ZakupkiModel
	{
		public int Id { get; set; }
		public int AgentId { get; set; }
		public int TransactionId { get; set; }
		public int CurrencyId { get; set; }

		public string Datetime { get; set; } = string.Empty;
		public double TotalSum { get; set; }
		public string? Comment { get; set; }

		public string AgentName { get; set; } = string.Empty;
		public string CurrencyName { get; set; } = string.Empty;
		private string _currencySign = string.Empty;
		public string CurrencySign 
		{ 
			get
			{
				return _currencySign;
			}
			set
			{
				if (string.IsNullOrEmpty(value) || value == " ")
					_currencySign = value.Substring(0, 3) + '.';
				else
					_currencySign = value;
			}
		}
	}
}
