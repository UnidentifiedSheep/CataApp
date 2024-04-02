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
		Task EditCatalogue(CatalogueModel catalogue, List<int> deleteIds);
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
		Task AddNewZakupka(IEnumerable<ZakupkaAltModel> zakupka, ZakupkiModel zakMain);
		Task<IEnumerable<CatalogueModel>> AddNewPricesForParts(IEnumerable<ZakupkaAltModel> parts, int currencyId);
		Task DeleteZakupkaByTransactionId(int transactionId);
		Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithCountReCalc(int transactionId, IEnumerable<ZakupkaAltModel> zakupkaAltModels);
		Task<IEnumerable<CatalogueModel>> EditZakupka(IEnumerable<int> deletedIds, IEnumerable<ZakupkaAltModel> zakupkaAlts, Dictionary<int, int> lastCounts, CurrencyModel currency, string date, double TotalSum, int transactionId);
		Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models, ProdajaModel mainModel);
		Task<IEnumerable<CatalogueModel>> DeleteProdajaCountReCalc(int transactionId, IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId);
	}
}
