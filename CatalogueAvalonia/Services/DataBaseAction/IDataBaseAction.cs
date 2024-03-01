using CatalogueAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public interface IDataBaseAction
	{
		
		Task DeleteMainNameById(int? id);
		
		Task DeleteFromMainCatById(int? id);
		Task EditCatalogue(CatalogueModel catalogue);
		Task<int> AddCatalogue(CatalogueModel catalogueModel);
	}
}
