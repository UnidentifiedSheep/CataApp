using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public interface IDataBaseProvider
	{
		Task<IEnumerable<CatalogueModel>> GetCatalogueAsync();
		Task<IEnumerable<ProducerModel>> GetProducersAsync();
		Task<CatalogueModel> GetCatalogueById(int uniId);
		Task<IEnumerable<AgentModel>> GetAgentsAsync();
		Task<IEnumerable<CurrencyModel>> GetCurrenciesAsync();
		Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsAsync();
		Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsByIdsAsync(int agentId, int currencyId, string startDate, string endDate);
		Task<AgentModel> GetAgentByIdAsync(int id);
	}
}
