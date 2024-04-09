using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogueAvalonia.Models;

namespace CatalogueAvalonia.Services.DataBaseAction;

public interface IDataBaseProvider
{
    Task<IEnumerable<CatalogueModel>> GetCatalogueAsync();
    Task<IEnumerable<ProducerModel>> GetProducersAsync();
    Task<CatalogueModel> GetCatalogueById(int uniId);
    Task<IEnumerable<CatalogueModel>> GetCatalogueById(IEnumerable<int> uniIds);
    Task<IEnumerable<AgentModel>> GetAgentsAsync();
    Task<IEnumerable<CurrencyModel>> GetCurrenciesAsync();
    Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsAsync();

    Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsByIdsAsync(int agentId, int currencyId,
        string startDate, string endDate);

    Task<AgentModel> GetAgentByIdAsync(int id);
    Task<AgentTransactionModel> GetLastAddedTransaction(int agentId, int currencyId);
    Task<IEnumerable<MainCatPriceModel>> GetMainCatPricesById(int mainCatId);
    Task<IEnumerable<ZakupkiModel>> GetZakupkiMainModel(string _startD, string _endD, int agentId);
    Task<IEnumerable<ZakupkaAltModel>> GetZakupkiAltModel(int zakMainGroupId);
    Task<IEnumerable<ProdajaModel>> GetProdajaMainGroup(string _startD, string _endD, int agentId);
    Task<IEnumerable<ProdajaAltModel>> GetProdajaAltModel(int zakMainGroupId);
}