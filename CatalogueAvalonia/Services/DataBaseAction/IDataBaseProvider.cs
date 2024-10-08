﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CatalogueAvalonia.Models;
using DataBase.Data;

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
    Task<IEnumerable<ProdajaAltModel>> GetProdajaAltModel(int zakMainGroupId, int action);
    Task<Bitmap?> GetPartsImg(int? mainCatId);
    Task<ProducerModel?> GetProducerById(int producerId);
    Task<decimal> GetAgentsBalance(int agentId, int currencyId);
    Task<IEnumerable<ProdajaAltModel>> GetProdajaAltModels(IEnumerable<int> ids);
    Task<IEnumerable<AgentBalance>> GetAgentBalances(int agentId);
    Task<bool> CanDeleteAgent(int agentId);
    Task<bool> CanDeleteGroup(int uniId);
    Task<bool> CanDeleteMainCat(int mainCatId);

}