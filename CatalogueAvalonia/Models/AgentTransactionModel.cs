using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class AgentTransactionModel
	{
		public int Id { get; set; }
		public int AgentId { get; set; }
		public int TransactionStatus { get; set; }
		private double _transactionSum;
		public double TransactionSum
		{
			get
			{
				return _transactionSum;
			}
			set
			{
				_transactionSum = value;
				if (TransactionStatus == 0 || TransactionStatus == 2)
					Summa = value;
				else if(TransactionStatus == 1 || TransactionStatus == 4)
					SummaPlateja = value;
			}
		}
		public double Balance { get; set; }
		public double Summa { get; set; }
		public double SummaPlateja { get; set; }
		public int CurrencyId { get; set; }
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
				if (!string.IsNullOrEmpty(value) || value == " ")
					_currencySign = value;
				else
					_currencySign = CurrencyName[..3];
			}
		}
		public string TransactionDatatime { get; set; } = string.Empty;
	}
}
