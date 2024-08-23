using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Avalonia.Media.Imaging;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using DataBase.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogueAvalonia.Services.DataBaseAction;

public class DataBaseProvider : IDataBaseProvider
{
    private readonly DataContextDataProvider _context;
    private readonly DataContextDataForInvoices _contextForInvoice;
    public DataBaseProvider(DataContextDataProvider dataContext, DataContextDataForInvoices contextForInvoice)
    {
        _context = dataContext;
        _contextForInvoice = contextForInvoice;
    }

    public async Task<IEnumerable<AgentModel>> GetAgentsAsync()
    {
        return await _context.Agents.Select(x => new AgentModel
        {
            Id = x.Id,
            IsZak = x.IsZak,
            Name = x.Name,
            OverPrice = x.OverPr,
            OverPriceText = x.OverPr.ToString()
        }).ToListAsync();
    }

    public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
    {
        var models = await _context.MainNames.Include(x => x.MainCats)
            .ThenInclude(x => x.MainCatPrices)
            .ThenInclude(x => x.Currency)
            .Include(x => x.MainCats)
            .ThenInclude(x => x.Producer)
            .ToListAsync();
        return models.Select(x => new CatalogueModel
        {
            UniId = x.UniId,
            Name = x.Name,
            MainCatId = null,
            RowColor = "#f2f2f2",
            Count = x.Count,
            Children = new ObservableCollection<CatalogueModel>(x.MainCats.Select(x => new CatalogueModel
            {
                UniId = x.UniId,
                UniValue = x.UniValue,
                Name = x.Name,
                Count = x.Count,
                ProducerId = x.ProducerId,
                ProducerName = x.Producer.ProducerName,
                MainCatId = x.Id,
                RowColor = x.RowColor,
                TextColor = x.TextColor,
                Children = new ObservableCollection<CatalogueModel>(x.MainCatPrices.Select(x => new CatalogueModel
                {
                    Name = "  Цена:",
                    UniId = null,
                    MainCatId = x.MainCatId,
                    PriceId = x.Id,
                    Count = x.Count,
                    Price = x.Price,
                    CurrencyId = x.CurrencyId,
                    CurrencyName = x.Currency.CurrencyName
                }))
            }))
        });
    }

    public async Task<CatalogueModel> GetCatalogueById(int uniId)
    {
        var model = await _context.MainNames.Where(x => x.UniId == uniId).Include(x => x.MainCats)
            .ThenInclude(x => x.MainCatPrices)
            .ThenInclude(x => x.Currency)
            .Include(x => x.MainCats)
            .ThenInclude(x => x.Producer).FirstOrDefaultAsync();
        if (model != null)
            return new CatalogueModel
            {
                
                UniId = model.UniId,
                Name = model.Name,
                MainCatId = null,
                RowColor = "#f2f2f2",
                Count = model.Count,
                Children = new ObservableCollection<CatalogueModel>(model.MainCats.Select(x => new CatalogueModel
                {
                    UniId = x.UniId,
                    UniValue = x.UniValue,
                    Name = x.Name,
                    Count = x.Count,
                    ProducerId = x.ProducerId,
                    ProducerName = x.Producer.ProducerName,
                    MainCatId = x.Id,
                    RowColor = x.RowColor,
                    TextColor = x.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(x.MainCatPrices.Select(x => new CatalogueModel
                    {
                        Name = "  Цена:",
                        UniId = null,
                        MainCatId = null,
                        PriceId = x.Id,
                        Count = x.Count,
                        Price = x.Price,
                        CurrencyId = x.CurrencyId,
                        CurrencyName = x.Currency.CurrencyName
                    }))
                }))
            };
        return new CatalogueModel();
    }

    public async Task<IEnumerable<CatalogueModel>> GetCatalogueById(IEnumerable<int> uniIds)
    {
        List<CatalogueModel> catalogueModels = new List<CatalogueModel>();
        foreach (var uniId in uniIds)
        {
            var model = await GetCatalogueById(uniId);
            catalogueModels.Add(model);
        }

        return catalogueModels;
    }

    public async Task<IEnumerable<CurrencyModel>> GetCurrenciesAsync()
    {
        return await _context.Currencies.Select(x => new CurrencyModel
        {
            CurrencyName = x.CurrencyName,
            Id = x.Id,
            ToUsd = x.ToUsd,
            CanDelete = x.CanDelete,
            CurrencySign = x.CurrencySign,
            IsDirty = false
        }).ToListAsync();
    }

    public async Task<IEnumerable<ProducerModel>> GetProducersAsync()
    {
        return await _context.Producers.Select(x => new ProducerModel { Id = x.Id, ProducerName = x.ProducerName })
            .ToListAsync();
    }

    public async Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsAsync()
    {
        return await _context.AgentTransactions.Select(x => new AgentTransactionModel
        {
            Id = x.Id,
            TransactionStatus = x.TransactionStatus,
            Balance = x.Balance,
            CurrencyId = x.Currency,
            TransactionDatatime = x.TransactionDatatime,
            AgentId = x.AgentId,
            TransactionSum = x.TransactionSum,
            ShortTime = x.Time.Insert(2, ":").Insert(5, ":")
        }).ToListAsync();
    }

    public async Task<IEnumerable<AgentTransactionModel>> GetAgentTransactionsByIdsAsync(int agentId, int currencyId,
        string startDate, string endDate)
    {
        FormattableString queryWithCurrency =
            $"SELECT * from agent_transactions where {startDate} <= transaction_datatime and {endDate} >= transaction_datatime and agent_id = {agentId} and currency = {currencyId}";
        FormattableString queryWithOutCurrency =
            $"SELECT * from agent_transactions where {startDate} <= transaction_datatime and {endDate} >= transaction_datatime and agent_id = {agentId}";


        if (currencyId == 1)
        {
            var transactions = await _context.AgentTransactions.FromSql(queryWithOutCurrency)
                .Include(x => x.CurrencyNavigation).ToListAsync().ConfigureAwait(false);
            return AgentTransactionToModel(transactions);
        }
        else
        {
            var transactions = await _context.AgentTransactions.FromSql(queryWithCurrency)
                .Include(x => x.CurrencyNavigation).ToListAsync().ConfigureAwait(false);
            return AgentTransactionToModel(transactions);
        }
    }

    public async Task<AgentModel> GetAgentByIdAsync(int id)
    {
        var model = await _context.Agents.FindAsync(id);
        if (model != null)
            return new AgentModel { Id = id, IsZak = model.IsZak, Name = model.Name, OverPrice = model.OverPr, OverPriceText = model.OverPr.ToString()};
        return new AgentModel();
    }

    public async Task<AgentTransactionModel> GetLastAddedTransaction(int agentId, int currencyId)
    {
        var lastTransaction = await _context.AgentTransactions
            .Where(x => x.AgentId == agentId && x.Currency == currencyId).Include(x => x.CurrencyNavigation)
            .OrderBy(x => x.Id).LastOrDefaultAsync();
        if (lastTransaction != null)
            return new AgentTransactionModel
            {
                Id = lastTransaction.Id,
                TransactionStatus = lastTransaction.TransactionStatus,
                Balance = lastTransaction.Balance,
                CurrencyId = lastTransaction.Currency,
                CurrencyName = lastTransaction.CurrencyNavigation.CurrencyName,
                TransactionDatatime = Converters.ToNormalDateTime(lastTransaction.TransactionDatatime),
                AgentId = lastTransaction.AgentId,
                TransactionSum = lastTransaction.TransactionSum,
                CurrencySign = lastTransaction.CurrencyNavigation.CurrencySign,
                ShortTime = lastTransaction.Time.Insert(2, ":").Insert(5, ":")
            };
        return new AgentTransactionModel();
    }

    public async Task<IEnumerable<MainCatPriceModel>> GetMainCatPricesById(int mainCatId)
    {
        return await _context.MainCatPrices.Where(x => x.MainCatId == mainCatId).Include(x => x.Currency).Select(x =>
            new MainCatPriceModel
            {
                Count = x.Count,
                CurrencyId = x.CurrencyId,
                Id = x.Id,
                MainCatId = x.MainCatId,
                Price = x.Price,
                IsDirty = false
            }).ToListAsync();
    }

    public async Task<IEnumerable<ZakupkiModel>> GetZakupkiMainModel(string _startD, string _endD, int agentId)
    {
        FormattableString queryWithOutAgent =
            $"SELECT * from zak_main_group where {_startD} <= datetime and {_endD} >= datetime";
        FormattableString queryWithAgent =
            $"SELECT * from zak_main_group where {_startD} <= datetime and {_endD} >= datetime and agent_id = {agentId}";

        if (agentId == 1)
            return await _context.ZakMainGroups.FromSql(queryWithOutAgent).Include(x => x.Agent)
                .Include(x => x.Currency).OrderByDescending(x => x.Id).Select(x => new ZakupkiModel
                {
                    AgentId = x.AgentId,
                    Id = x.Id,
                    CurrencyId = x.CurrencyId,
                    TransactionId = x.TransactionId,
                    AgentName = x.Agent.Name,
                    CurrencyName = x.Currency.CurrencyName,
                    CurrencySign = x.Currency.CurrencySign,
                    Datetime = Converters.ToNormalDateTime(x.Datetime),
                    TotalSum = x.TotalSum,
                    Comment = x.Comment
                }).ToListAsync();
        return await _context.ZakMainGroups.FromSql(queryWithAgent).Include(x => x.Agent).Include(x => x.Currency)
            .OrderByDescending(x => x.Id).Select(x => new ZakupkiModel
            {
                AgentId = x.AgentId,
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                TransactionId = x.TransactionId,
                AgentName = x.Agent.Name,
                CurrencyName = x.Currency.CurrencyName,
                Datetime = Converters.ToNormalDateTime(x.Datetime),
                TotalSum = x.TotalSum,
                CurrencySign = x.Currency.CurrencySign,
                Comment = x.Comment
            }).ToListAsync();
    }

    public async Task<IEnumerable<ZakupkaAltModel>> GetZakupkiAltModel(int zakMainGroupId)
    {
        var model = await _context.Zakupkas.Include(x => x.MainCat).Where(x => x.ZakId == zakMainGroupId).ToListAsync();
        var list = new List<ZakupkaAltModel>();
        foreach (var item in model)
            if (item.MainCat != null)
                list.Add(new ZakupkaAltModel
                {
                    Id = item.Id,
                    Count = item.Count,
                    TextCount = Convert.ToString(item.Count),
                    MainCatId = item.MainCatId,
                    MainCatName = item.MainCat.Name,
                    MainName = item.MainName,
                    Price = item.Price,
                    TextDecimal = Convert.ToString(item.Price),
                    UniValue = item.MainCat.UniValue,
                    ZakupkaId = item.ZakId
                });
            else
                list.Add(new ZakupkaAltModel
                {
                    Id = item.Id,
                    Count = item.Count,
                    TextCount = Convert.ToString(item.Count),
                    MainCatId = item.MainCatId,
                    MainCatName = null,
                    MainName = item.MainName,
                    Price = item.Price,
                    TextDecimal = Convert.ToString(item.Price),
                    UniValue = item.UniValue,
                    ZakupkaId = item.ZakId
                });
        return list;
    }

    public async Task<IEnumerable<ProdajaModel>> GetProdajaMainGroup(string _startD, string _endD, int agentId)
    {
        FormattableString queryWithOutAgent =
            $"SELECT * from prod_main_group where {_startD} <= datetime and {_endD} >= datetime";
        FormattableString queryWithAgent =
            $"SELECT * from prod_main_group where {_startD} <= datetime and {_endD} >= datetime and agent_id = {agentId}";

        if (agentId == 1)
            return await _context.ProdMainGroups.FromSql(queryWithOutAgent).Include(x => x.Agent)
                .Include(x => x.Currency).OrderByDescending(x => x.Id).Select(x => new ProdajaModel
                {
                    AgentId = x.AgentId,
                    Id = x.Id,
                    CurrencyId = x.CurrencyId,
                    TransactionId = x.TransactionId,
                    AgentName = x.Agent.Name,
                    CurrencyName = x.Currency.CurrencyName,
                    CurrencySign = x.Currency.CurrencySign,
                    Datetime = Converters.ToNormalDateTime(x.Datetime),
                    TotalSum = x.TotalSum,
                    Comment = x.Comment
                }).ToListAsync();
        return await _context.ProdMainGroups.FromSql(queryWithAgent).Include(x => x.Agent).Include(x => x.Currency)
            .OrderByDescending(x => x.Id).Select(x => new ProdajaModel
            {
                AgentId = x.AgentId,
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                TransactionId = x.TransactionId,
                AgentName = x.Agent.Name,
                CurrencyName = x.Currency.CurrencyName,
                Datetime = Converters.ToNormalDateTime(x.Datetime),
                TotalSum = x.TotalSum,
                CurrencySign = x.Currency.CurrencySign,
                Comment = x.Comment
            }).ToListAsync();
    }
    public async Task<IEnumerable<ProdajaAltModel>> GetProdajaAltModel(int zakMainGroupId, int action)
    {
        if (action == 1)
        {
            var model = await _contextForInvoice.Prodajas.Include(x => x.MainCat).ThenInclude(x => x.Producer)
                .Where(x => x.ProdajaId == zakMainGroupId).ToListAsync();
            var list = new List<ProdajaAltModel>();
            foreach (var item in model)
                if (item.MainCat != null)
                    list.Add(new ProdajaAltModel
                    {
                        Id = item.Id,
                        MaxCount = item.Count,
                        Count = item.Count,
                        TextCont = Convert.ToString(item.Count),
                        MainCatId = item.MainCatId,
                        MainCatName = item.MainCat.Name,
                        MainName = item.MainName,
                        Price = item.Price,
                        TextDecimal = Convert.ToString(item.Price),
                        UniValue = item.MainCat.UniValue,
                        ProdajaId = item.ProdajaId,
                        ProducerName = item.MainCat.Producer.ProducerName,
                        InitialPrice = item.InitialPrice,
                        CurrencyInitialId = item.CurrencyId,
                        Comment = item.Comment
                    });
                else
                    list.Add(new ProdajaAltModel
                    {
                        Id = item.Id,
                        Count = item.Count,
                        MaxCount = item.Count,
                        TextCont = Convert.ToString(item.Count),
                        MainCatId = item.MainCatId,
                        MainCatName = null,
                        MainName = item.MainName,
                        Price = item.Price,
                        TextDecimal = Convert.ToString(item.Price),
                        UniValue = item.UniValue,
                        ProdajaId = item.ProdajaId,
                        Comment = item.Comment
                    });
            return list;
        }
        else
        {
            var model = await _context.Prodajas.Include(x => x.MainCat).ThenInclude(x => x.Producer)
                .Where(x => x.ProdajaId == zakMainGroupId).ToListAsync();
            var list = new List<ProdajaAltModel>();
            foreach (var item in model)
                if (item.MainCat != null)
                    list.Add(new ProdajaAltModel
                    {
                        Id = item.Id,
                        MaxCount = item.Count,
                        Count = item.Count,
                        TextCont = Convert.ToString(item.Count),
                        MainCatId = item.MainCatId,
                        MainCatName = item.MainCat.Name,
                        MainName = item.MainName,
                        Price = item.Price,
                        TextDecimal = Convert.ToString(item.Price),
                        UniValue = item.MainCat.UniValue,
                        ProdajaId = item.ProdajaId,
                        ProducerName = item.MainCat.Producer.ProducerName,
                        InitialPrice = item.InitialPrice,
                        CurrencyInitialId = item.CurrencyId,
                        Comment = item.Comment
                    });
                else
                    list.Add(new ProdajaAltModel
                    {
                        Id = item.Id,
                        Count = item.Count,
                        MaxCount = item.Count,
                        TextCont = Convert.ToString(item.Count),
                        MainCatId = item.MainCatId,
                        MainCatName = null,
                        MainName = item.MainName,
                        Price = item.Price,
                        TextDecimal = Convert.ToString(item.Price),
                        UniValue = item.UniValue,
                        ProdajaId = item.ProdajaId,
                        Comment = item.Comment
                    });
            return list;
        }
    }

    private IEnumerable<AgentTransactionModel> AgentTransactionToModel(IEnumerable<AgentTransaction> transactions)
    {
        return transactions.Select(x => new AgentTransactionModel
        {
            Id = x.Id,
            TransactionStatus = x.TransactionStatus,
            Balance = x.Balance,
            CurrencyId = x.Currency,
            CurrencyName = x.CurrencyNavigation.CurrencyName,
            TransactionDatatime = Converters.ToNormalDateTime(x.TransactionDatatime),
            AgentId = x.AgentId,
            TransactionSum = x.TransactionSum,
            CurrencySign = x.CurrencyNavigation.CurrencySign,
            ShortTime = x.Time.Insert(2, ":").Insert(5, ":")
        });
    }

    public async Task<Bitmap?> GetPartsImg(int? mainCatId)
    {
        var part = await _context.MainCats.FindAsync(mainCatId);
        if (part != null)
        {
            if (part.ImageId != null)
            {
                Bitmap? img;
                var imgModel = await _context.Images.FindAsync(part.ImageId);
                if (imgModel != null && imgModel.Img != null)
                {
                    using (var ms = new MemoryStream(imgModel.Img))
                        img = new Bitmap(ms);
                }
                else
                    return null;

                return img;
            }
            else
                return null;
        }
        else
            return null;
    }

    public async Task<ProducerModel?> GetProducerById(int producerId)
    {
        var producer = await _context.Producers.FindAsync(producerId);
        if (producer != null)
            return new ProducerModel { Id = producer.Id, ProducerName = producer.ProducerName };
        else
            return null;
    }

    public async Task<decimal> GetAgentsBalance(int agentId, int currencyId)
    {
        var agent = await _context.AgentBalances.FirstOrDefaultAsync(x =>
            x.AgentId == agentId && x.CurrencyId == currencyId);
        return agent?.Balance ?? 0m;
    }
}