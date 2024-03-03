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
		public async Task<AgentModel> GetAgentByIdAsync(int id)
		{
			return await _dataBaseProvider.GetAgentByIdAsync(id).ConfigureAwait(false);
		}
	}
}
