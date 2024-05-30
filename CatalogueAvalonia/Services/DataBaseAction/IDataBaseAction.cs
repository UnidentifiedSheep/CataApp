using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;

namespace CatalogueAvalonia.Services.DataBaseAction;

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
    Task<CatalogueModel?> EditMainCatPrices(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId, int endCount);
    Task DeleteMainCatPricesById(int mainCatId);
    Task EditCurrency(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds);
    Task DeleteAllCurrencies();
    Task AddNewZakupka(IEnumerable<ZakupkaAltModel> zakupka, ZakupkiModel zakMain);
    Task<IEnumerable<CatalogueModel>> AddNewPricesForParts(IEnumerable<ZakupkaAltModel> parts, int currencyId);
    Task DeleteZakupkaByTransactionId(int transactionId);

    Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithCountReCalc(int transactionId,
        IEnumerable<ZakupkaAltModel> zakupkaAltModels);

    Task<IEnumerable<CatalogueModel>> EditZakupka(IEnumerable<int> deletedIds, IEnumerable<ZakupkaAltModel> zakupkaAlts,
        Dictionary<int, int> lastCounts, CurrencyModel currency, string date, decimal totalSum, int transactionId,
        string comment);

    Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models, ProdajaModel mainModel);

    Task<IEnumerable<CatalogueModel>> DeleteProdajaCountReCalc(int transactionId,
        IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId);

    Task<IEnumerable<CatalogueModel>> EditProdaja(IEnumerable<Tuple<int, decimal>> deletedIds,
        IEnumerable<ProdajaAltModel> prodajaAltModels, Dictionary<int, int> lastCounts, CurrencyModel currency,
        string date, decimal totalSum, int transactionId, string comment);

    Task<int?> CheckCanDeleteProdaja(int? mainCatId);
    Task SetMainCatImg(int? mainCatId, byte[]? img);
    Task EditProducerById(int producerId, string newName);
    Task<ProducerModel?> AddNewProducer(string producerName);
    Task<bool> DeleteProducer(int producerId);
    Task<CatalogueModel?> EditColor(string rowColor, int id);
}