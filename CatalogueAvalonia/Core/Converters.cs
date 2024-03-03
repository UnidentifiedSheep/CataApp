using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Core
{
	public static class Converters
	{
		public static string ToDateTimeSqlite(string datetime)
		{
			string res = "";
			if (datetime.Length == 10)
			{
				res += datetime.Substring(6, 4);
				res += datetime.Substring(3, 2);
				res += datetime.Substring(0, 2);
				return res;
			}
			else
			{
				return "00000000";
			}
		}
		public static string ToNormalDateTime(string date)
		{
			string res = "";
			if (date.Length == 8)
			{
				res += date.Substring(6, 2) + '.';
				res += date.Substring(4, 2) + '.';
				res += date.Substring(0, 4);
				return res;
			}
			else
				return "00.00.0000";
		}
	}
}
