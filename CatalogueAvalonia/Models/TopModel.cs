using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Services.DataBaseAction;
using MsBox.Avalonia;
using Serilog;

namespace CatalogueAvalonia.Models;

public class TopModel
{
    private readonly IDataBaseAction _dataBaseAction;
    private readonly IDataBaseProvider _dataBaseProvider;
    private readonly ILogger _logger;
    private readonly TaskQueue _taskQueue;

    public TopModel(IDataBaseProvider dataBaseProvider, IDataBaseAction dataBaseAction, ILogger logger)
    {
        _dataBaseProvider = dataBaseProvider;
        _dataBaseAction = dataBaseAction;
        _logger = logger;
        _taskQueue = new TaskQueue();
    }

    /// Исправить _task.queue. вызывается в основном потоке.
    /// <summary>
    ///     Получает все запчасти из каталога.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
    {
        try
        {
            return await _dataBaseProvider.GetCatalogueAsync();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    /// <summary>
    ///     Удаляет основную группу запчастей по id основной группы.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteGroupFromCatalogue(int? id)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.DeleteMainNameById(id));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Удаляет запчасть по id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteSoloFromCatalogue(int? id)
    {
        try
        {
            await _dataBaseAction.DeleteFromMainCatById(id);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Получает всех производителей из базы данных.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ProducerModel>> GetProducersAsync()
    {
        try
        {
            return await _dataBaseProvider.GetProducersAsync();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ProducerModel>();
        }
    }

    /// <summary>
    ///     Редактирует группу запчастей.
    /// </summary>
    /// <param name="catalogue"></param>
    /// <param name="deleteIds"></param>
    public async Task EditCatalogueAsync(CatalogueModel catalogue, List<int> deleteIds)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.EditCatalogue(catalogue, deleteIds));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Получает группу запчастей по основному id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CatalogueModel> GetCatalogueByIdAsync(int id)
    {
        try
        {
            return await _dataBaseProvider.GetCatalogueById(id);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new CatalogueModel();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> GetCatalogueByIdAsync(IEnumerable<int> ids)
    {
        try
        {
            return await _dataBaseProvider.GetCatalogueById(ids);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    /// <summary>
    ///     Добавление новой группы в катало запчастей.
    /// </summary>
    /// <param name="catalogueModell"></param>
    /// <returns>Id только что добавленной группы.</returns>
    public async Task<int> AddNewCatalogue(CatalogueModel catalogueModell)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddCatalogue(catalogueModell));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return -1;
        }
    }

    /// <summary>
    ///     Получает всеx контрагентов из базы данных.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<AgentModel>> GetAllAgentsAsync()
    {
        try
        {
            return await _dataBaseProvider.GetAgentsAsync();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<AgentModel>();
        }
    }

    /// <summary>
    ///     Получает все валюты из базы данных.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<CurrencyModel>> GetAllCurrenciesAsync()
    {
        try
        {
            return await _dataBaseProvider.GetCurrenciesAsync();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CurrencyModel>();
        }
    }

    /// <summary>
    ///     Получает все транзакции из базы данных.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<AgentTransactionModel>> GetAllAgentTransactionsAsync()
    {
        try
        {
            return await _dataBaseProvider.GetAgentTransactionsAsync();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<AgentTransactionModel>();
        }
    }

    /// <summary>
    ///     Получает транзакции выбранного контрагента за определенный период времени.
    /// </summary>
    /// <param name="agentId">id контрагента.</param>
    /// <param name="currencyId">id валюты. Если все валюты то 0.</param>
    /// <param name="sartDate">С какой даты.</param>
    /// <param name="endDate">По какую дату.</param>
    /// <returns></returns>
    public async Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsByIdsAsync(int agentId, int currencyId,
        string sartDate, string endDate)
    {
        try
        {
            return await _dataBaseProvider.GetAgentTransactionsByIdsAsync(agentId, currencyId, sartDate, endDate);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<AgentTransactionModel>();
        }
    }

    /// <summary>
    ///     Изменяет название или возможность закупки Контрагента.
    /// </summary>
    /// <param name="agentModel"></param>
    /// <returns></returns>
    public async Task EditAgentAsync(AgentModel agentModel)
    {
        try
        {
            await _dataBaseAction.EditAgent(agentModel);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Добавление нового контрагента.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="isZak">Закупочный или нет.</param>
    /// <returns></returns>
    public async Task<AgentModel> AddNewAgentAsync(string name, int isZak)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewAgent(name, isZak));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new AgentModel();
        }
    }

    /// <summary>
    ///     Получение контрагента по id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AgentModel> GetAgentByIdAsync(int id)
    {
        try
        {
            return await _dataBaseProvider.GetAgentByIdAsync(id);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new AgentModel();
        }
    }

    /// <summary>
    ///     Удаление контрагента по id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAgentAsync(int id)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.DeleteAgent(id));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Добавление нвой транзакции.
    /// </summary>
    /// <param name="agentTransactionModel"></param>
    /// <returns></returns>
    public async Task<int> AddNewTransactionAsync(AgentTransactionModel agentTransactionModel)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewTransaction(agentTransactionModel));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return -1;
        }
    }

    /// <summary>
    ///     Получение последней транзакции.
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="currencyId"></param>
    /// <returns></returns>
    public async Task<AgentTransactionModel> GetLastTransactionAsync(int agentId, int currencyId)
    {
        try
        {
            return await _dataBaseProvider.GetLastAddedTransaction(agentId, currencyId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new AgentTransactionModel();
        }
    }

    /// <summary>
    ///     Удаляет транзакцию и меняет Сальдо последующих операций.
    /// </summary>
    /// <param name="agentId"></param>
    /// <param name="currencyId"></param>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    public async Task DeleteAgentTransactionAsync(int agentId, int currencyId, int transactionId)
    {
        try
        {
            await _taskQueue.Enqueue(async () =>
                await _dataBaseAction.DeleteTransaction(agentId, currencyId, transactionId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Returns prices of part.
    /// </summary>
    /// <param name="mainCatId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<MainCatPriceModel>> GetMainCatPricesByIdAsync(int mainCatId)
    {
        try
        {
            return await _dataBaseProvider.GetMainCatPricesById(mainCatId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<MainCatPriceModel>();
        }
    }

    /// <summary>
    ///     Редактирует цены и количество запчасти.
    /// </summary>
    /// <param name="mainCatPrices"></param>
    /// <param name="mainCatId"></param>
    /// <returns></returns>
    public async Task<CatalogueModel?> EditMainCatPricesAsync(IEnumerable<MainCatPriceModel> mainCatPrices,
        int mainCatId, int endCount)
    {
        try
        {
            return await _taskQueue.Enqueue(async () =>
                await _dataBaseAction.EditMainCatPrices(mainCatPrices, mainCatId, endCount));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new CatalogueModel();
        }
    }

    /// <summary>
    ///     Удаляет цены запчастей по id.
    /// </summary>
    /// <param name="mainCatId"></param>
    /// <returns></returns>
    public async Task DeleteMainCatPricesByIdAsync(int mainCatId)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.DeleteMainCatPricesById(mainCatId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Изменяет валюты.
    /// </summary>
    /// <param name="currencyModels"></param>
    /// <param name="deletedIds">Ids которые были удалены</param>
    /// <returns></returns>
    public async Task EditCurrenciesAsync(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.EditCurrency(currencyModels, deletedIds));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    /// <summary>
    ///     Удаляет все валюты кроме "Доллара" и "Неизвестной".
    /// </summary>
    /// <returns></returns>
    public async Task DeleteAllCurrenciesAsync()
    {
        try
        {
            await _dataBaseAction.DeleteAllCurrencies();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    public async Task<IEnumerable<ZakupkiModel>> GetZakupkiMainGroupAsync(string startD, string endD, int agentId)
    {
        try
        {
            return await _dataBaseProvider.GetZakupkiMainModel(startD, endD, agentId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ZakupkiModel>();
        }
    }

    public async Task AddNewZakupkaAsync(IEnumerable<ZakupkaAltModel> zakupkaAlts, ZakupkiModel zakupkiModel)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewZakupka(zakupkaAlts, zakupkiModel));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> AddNewPricesForPartsAsync(IEnumerable<ZakupkaAltModel> catas,
        int currencyId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewPricesForParts(catas, currencyId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<IEnumerable<ZakupkaAltModel>> GetZakAltGroup(int mainGroupId)
    {
        try
        {
            return await _dataBaseProvider.GetZakupkiAltModel(mainGroupId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ZakupkaAltModel>();
        }
    }

    public async Task DeleteZakupkaByTransactionIdAsync(int transactionId)
    {
        try
        {
            await _dataBaseAction.DeleteZakupkaByTransactionId(transactionId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithPricesReCount(int transactionId,
        IEnumerable<ZakupkaAltModel> zakupkaAltModels)
    {
        try
        {
            return await _taskQueue.Enqueue(async () =>
                await _dataBaseAction.DeleteZakupkaWithCountReCalc(transactionId, zakupkaAltModels)
            );
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> EditZakupkaAsync(IEnumerable<Tuple<int,int>> deletedIds,
        IEnumerable<ZakupkaAltModel> zakupkaAlts, Dictionary<int, int> lastCounts,
        CurrencyModel currency, decimal totalSum, string date, int transactionId, string comment)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.EditZakupka(deletedIds, zakupkaAlts,
                lastCounts, currency, date, totalSum, transactionId, comment));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<IEnumerable<ProdajaModel>> GetProdajaMainGroupAsync(string startD, string endD, int agentId)
    {
        try
        {
            return await _dataBaseProvider.GetProdajaMainGroup(startD, endD, agentId);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ProdajaModel>();
        }
    }

    public async Task<IEnumerable<ProdajaAltModel>> GetProdajaAltGroupAsync(int mainGroupId)
    {
        try
        {
            return await _dataBaseProvider.GetProdajaAltModel(mainGroupId, 0);
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ProdajaAltModel>();
        }
    }
    public async Task<IEnumerable<ProdajaAltModel>> GetProdajaAltGroupNewTask(int mainGroupId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseProvider.GetProdajaAltModel(mainGroupId, 1));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<ProdajaAltModel>();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models,
        ProdajaModel mainModel)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewProdaja(models, mainModel));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteProdajaAsync(int transactionId,
        IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () =>
                await _dataBaseAction.DeleteProdajaCountReCalc(transactionId, prodajaAltModels, currencyId)
            );
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> EditProdajaAsync(IEnumerable<Tuple<int, decimal>> deletedIds,
        IEnumerable<ProdajaAltModel> prodajaAltModels, Dictionary<int, int> lastCounts, CurrencyModel currency,
        string date, decimal totalSum, int transactionId, string comment)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction
                .EditProdaja(deletedIds, prodajaAltModels, lastCounts, currency, date, totalSum, transactionId, comment)
            );
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return new List<CatalogueModel>();
        }
    }

    public async Task<int?> CanDeleteProdaja(int? mainCatId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction
                .CheckCanDeleteProdaja(mainCatId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return 0;
        }
    }

    public async Task<Bitmap?> GetPartsImg(int? mainCatId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseProvider
                .GetPartsImg(mainCatId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return null;
        }
    }

    public async Task SetPartsImg(int? mainCatId, byte[]? img)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction
                .SetMainCatImg(mainCatId, img));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    public async Task<ProducerModel?> GetProducerById(int producerId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseProvider.GetProducerById(producerId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return null;
        }
    }

    public async Task EditProducerAsync(int producerId, string newName)
    {
        try
        {
            await _taskQueue.Enqueue(async () => await _dataBaseAction.EditProducerById(producerId, newName));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
        }
    }

    public async Task<ProducerModel?> AddNewProducer(string newProducersName)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.AddNewProducer(newProducersName));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return null;
        }
    }

    public async Task<bool> DeleteProducer(int producerId)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.DeleteProducer(producerId));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return false;
        }
    }

    public async Task<CatalogueModel?> EditColor(string rowColor, string textColor, int id)
    {
        try
        {
            return await _taskQueue.Enqueue(async () => await _dataBaseAction.EditColor(rowColor, textColor, id));
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка: \"{e}\"?").ShowWindowAsync();
            return null;
        }
    }
}