using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public interface IDataBaseProvider
	{
		Task<IEnumerable<CatalogueModel>> GetCatalogueAsync();
		Task<IEnumerable<ProducerModel>> GetProducersAsync();
		Task<CatalogueModel> GetCatalogueById(int uniId);
	}
}
