using CatalogueAvalonia.Models;
using DataBase.Data;
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
		Task EditAgent(AgentModel agentModel);
		Task<AgentModel> AddNewAgent(string name, int isZak);
		Task DeleteAgent(int id);
		Task<int> AddNewTransaction(AgentTransactionModel agentTransaction);
		Task DeleteTransaction(int agentId, int currencyId, int transactionId);
		Task<CatalogueModel?> EditMainCatPrices(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId);
		Task DeleteMainCatPricesById(int mainCatId);
		Task EditCurrency(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds);
		Task DeleteAllCurrencies();
	}
}
