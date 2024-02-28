using CatalogueAvalonia.Services.DataBaseAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class TopModel
	{
		private readonly IDataBaseProvider _dataBaseProvider;
		public TopModel()
		{
			_dataBaseProvider = new DataBaseProvider();
		}
		public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
		{
			return await _dataBaseProvider.GetCatalogueAsync().ConfigureAwait(false);
		}
	}
}
