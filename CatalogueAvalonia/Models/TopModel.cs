using CatalogueAvalonia.Model;
using CatalogueAvalonia.Services.DataBaseAction;
using DataBase.Data;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class TopModel
	{
		private readonly IDataBaseProvider _dataBaseProvider;
		private readonly IDataBaseAction _dataBaseAction;
		public TopModel(IDataBaseProvider dataBaseProvider, IDataBaseAction dataBaseAction)
		{
			_dataBaseProvider = dataBaseProvider;
			_dataBaseAction = dataBaseAction;
		}
		/// <summary>
		/// Получает все запчасти из каталога.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
		{
			return await _dataBaseProvider.GetCatalogueAsync();
		}
		/// <summary>
		/// Удаляет основную группу запчастей по id основной группы.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task DeleteGroupFromCatalogue(int? id)
		{
			await _dataBaseAction.DeleteMainNameById(id);
		}
		/// <summary>
		/// Удаляет запчасть по id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task DeleteSoloFromCatalogue(int? id)
		{
			await _dataBaseAction.DeleteFromMainCatById(id);
		}
		/// <summary>
		/// Получает всех производителей из базы данных.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<ProducerModel>> GetProducersAsync()
		{
			return await _dataBaseProvider.GetProducersAsync();
		}
		/// <summary>
		/// Редактирует группу запчастей.
		/// </summary>
		/// <param name="catalogue"></param>
		/// <returns></returns>
		public async Task EditCatalogueAsync(CatalogueModel catalogue)
		{
			await _dataBaseAction.EditCatalogue(catalogue).ConfigureAwait(false);
		}
		/// <summary>
		/// Получает группу запчастей по основному id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<CatalogueModel> GetCatalogueByIdAsync(int id)
		{
			return await _dataBaseProvider.GetCatalogueById(id).ConfigureAwait(false);
		}
		/// <summary>
		/// Добавление новой группы в катало запчастей.
		/// </summary>
		/// <param name="catalogueModell"></param>
		/// <returns>Id только что добавленной группы.</returns>
		public async Task<int> AddNewCatalogue(CatalogueModel catalogueModell)
		{
			return await _dataBaseAction.AddCatalogue(catalogueModell);
		}
		/// <summary>
		/// Получает всеx контрагентов из базы данных.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<AgentModel>> GetAllAgentsAsync()
		{
			return await _dataBaseProvider.GetAgentsAsync().ConfigureAwait(false);
		}
		/// <summary>
		/// Получает все валюты из базы данных.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<CurrencyModel>> GetAllCurrenciesAsync()
		{
			return await _dataBaseProvider.GetCurrenciesAsync().ConfigureAwait(false);
		}
		/// <summary>
		/// Получает все транзакции из базы данных.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<AgentTransactionModel>> GetAllAgentTransactionsAsync()
		{
			return await _dataBaseProvider.GetAgentTransactionsAsync().ConfigureAwait(false);
		}
		/// <summary>
		/// Получает транзакции выбранного контрагента за определенный период времени.
		/// </summary>
		/// <param name="agentId">id контрагента.</param>
		/// <param name="currencyId">id валюты. Если все валюты то 0.</param>
		/// <param name="sartDate">С какой даты.</param>
		/// <param name="endDate">По какую дату.</param>
		/// <returns></returns>
		public async Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsByIdsAsync(int agentId, int currencyId, string sartDate, string endDate)
		{
			return await _dataBaseProvider.GetAgentTransactionsByIdsAsync(agentId, currencyId, sartDate, endDate).ConfigureAwait(false);
		}
		/// <summary>
		/// Изменяет название или возможность закупки Контрагента.
		/// </summary>
		/// <param name="agentModel"></param>
		/// <returns></returns>
		public async Task EditAgentAsync(AgentModel agentModel)
		{
			await _dataBaseAction.EditAgent(agentModel).ConfigureAwait(false);
		}
		/// <summary>
		/// Добавление нового контрагента.
		/// </summary>
		/// <param name="name">Имя</param>
		/// <param name="isZak">Закупочный или нет.</param>
		/// <returns></returns>
		public async Task<AgentModel> AddNewAgentAsync(string name, int isZak)
		{
			return await _dataBaseAction.AddNewAgent(name, isZak).ConfigureAwait(false);
		}
		/// <summary>
		/// Получение контрагента по id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<AgentModel> GetAgentByIdAsync(int id)
		{
			return await _dataBaseProvider.GetAgentByIdAsync(id).ConfigureAwait(false);
		}
		/// <summary>
		/// Удаление контрагента по id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task DeleteAgentAsync(int id)
		{
			await _dataBaseAction.DeleteAgent(id).ConfigureAwait(false);
		}
		/// <summary>
		/// Добавление нвой транзакции.
		/// </summary>
		/// <param name="agentTransactionModel"></param>
		/// <returns></returns>
		public async Task<int> AddNewTransactionAsync(AgentTransactionModel agentTransactionModel)
		{
			return await _dataBaseAction.AddNewTransaction(agentTransactionModel).ConfigureAwait(false);
		}
		/// <summary>
		/// Получение последней транзакции.
		/// </summary>
		/// <param name="agentId"></param>
		/// <param name="currencyId"></param>
		/// <returns></returns>
		public async Task<AgentTransactionModel> GetLastTransactionAsync(int agentId, int currencyId)
		{
			return await _dataBaseProvider.GetLastAddedTransaction(agentId, currencyId).ConfigureAwait(false);
		}
		/// <summary>
		/// Удаляет транзакцию и меняет Сальдо последующих операций.
		/// </summary>
		/// <param name="agentId"></param>
		/// <param name="currencyId"></param>
		/// <param name="transactionId"></param>
		/// <returns></returns>
		public async Task DeleteAgentTransactionAsync(int agentId, int currencyId, int transactionId)
		{
			await _dataBaseAction.DeleteTransaction(agentId, currencyId, transactionId).ConfigureAwait(false);
		}
		/// <summary>
		/// Returns prices of part.
		/// </summary>
		/// <param name="mainCatId"></param>
		/// <returns></returns>
		public async Task<IEnumerable<MainCatPriceModel>> GetMainCatPricesByIdAsync(int mainCatId)
		{
			return await _dataBaseProvider.GetMainCatPricesById(mainCatId).ConfigureAwait(false);
		}
		/// <summary>
		/// Редактирует цены и количество запчасти.
		/// </summary>
		/// <param name="mainCatPrices"></param>
		/// <returns></returns>
		public async Task<CatalogueModel?> EditMainCatPricesAsync(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId)
		{
			return await _dataBaseAction.EditMainCatPrices(mainCatPrices, mainCatId).ConfigureAwait(false);
		}
		/// <summary>
		/// Удаляет цены запчастей по id.
		/// </summary>
		/// <param name="mainCatId"></param>
		/// <returns></returns>
		public async Task DeleteMainCatPricesByIdAsync(int mainCatId)
		{
			await _dataBaseAction.DeleteMainCatPricesById(mainCatId).ConfigureAwait(false);
		}
		/// <summary>
		/// Изменяет валюты.
		/// </summary>
		/// <param name="currencyModels"></param>
		/// <param name="deletedIds">Ids которые были удалены</param>
		/// <returns></returns>
		public async Task EditCurrenciesAsync(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds)
		{
			await _dataBaseAction.EditCurrency(currencyModels, deletedIds).ConfigureAwait(false);
		}
		/// <summary>
		/// Удаляет все валюты кроме "Доллара" и "Неизвестной".
		/// </summary>
		/// <returns></returns>
		public async Task DeleteAllCurrenciesAsync()
		{
			await _dataBaseAction.DeleteAllCurrencies().ConfigureAwait(false);
		}
	}
}
