using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using DataBase.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogueAvalonia.Services.DataBaseAction;

public class DataBaseAction : IDataBaseAction
{
    private readonly DataContext _context;

    public DataBaseAction(DataContext dataContext)
    {
        _context = dataContext;
    }

    public async Task DeleteMainNameById(int? id)
    {
        var el = await _context.MainNames.FindAsync(id);
        if (el != null)
        {
            _context.MainNames.Remove(el);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteFromMainCatById(int? id)
    {
        var el = await _context.MainCats.FindAsync(id);
        if (el != null)
        {
            _context.MainCats.Remove(el);
            await _context.SaveChangesAsync();
        }
    }

    public async Task EditCatalogue(CatalogueModel catalogue, List<int> deleteIds)
    {
        var uniId = catalogue.UniId;
        var mainName = await _context.MainNames.FindAsync(uniId);
        foreach (var id in deleteIds)
        {
            var mainCat = await _context.MainCats.FindAsync(id);
            if (mainCat != null) _context.Remove(mainCat);
        }

        if (mainName != null && catalogue.Children != null)
        {
            mainName.Name = catalogue.Name;

            foreach (var child in catalogue.Children)
            {
                var part = await _context.MainCats.FindAsync(child.MainCatId);
                if (part != null)
                {
                    part.UniValue = child.UniValue;
                    part.Name = child.Name;
                    part.ProducerId = child.ProducerId;
                }
                else
                {
                    await _context.MainCats.AddAsync(new MainCat
                    {
                        Count = 0,
                        Name = child.Name,
                        ProducerId = child.ProducerId,
                        UniValue = child.UniValue,
                        UniId = uniId ?? 5923
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> AddCatalogue(CatalogueModel catalogueModel)
    {
        if (catalogueModel.Children != null)
        {
            var model = new MainName
            {
                Name = catalogueModel.Name,
                MainCats = catalogueModel.Children.Select(x => new MainCat
                {
                    UniValue = x.UniValue,
                    Name = x.Name,
                    ProducerId = x.ProducerId
                }).ToList()
            };
            await _context.MainNames.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.UniId;
        }

        return 5923;
    }

    public async Task EditAgent(AgentModel agentModel)
    {
        var model = await _context.Agents.FindAsync(agentModel.Id);
        if (model != null)
        {
            model.Name = agentModel.Name;
            model.IsZak = agentModel.IsZak;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AgentModel> AddNewAgent(string name, int isZak)
    {
        var model = new Agent { Name = name, IsZak = isZak };
        await _context.Agents.AddAsync(model);
        await _context.SaveChangesAsync();

        return new AgentModel { Id = model.Id, IsZak = model.IsZak, Name = model.Name };
    }

    public async Task DeleteAgent(int id)
    {
        var model = await _context.Agents.FindAsync(id);
        if (model != null)
        {
            _context.Agents.Remove(model);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> AddNewTransaction(AgentTransactionModel agentTransaction)
    {
        var model = new AgentTransaction
        {
            AgentId = agentTransaction.AgentId,
            TransactionDatatime = Converters.ToDateTimeSqlite(agentTransaction.TransactionDatatime),
            TransactionStatus = agentTransaction.TransactionStatus,
            TransactionSum = agentTransaction.TransactionSum,
            Currency = agentTransaction.CurrencyId,
            Balance = agentTransaction.Balance
        };
        await _context.AgentTransactions.AddAsync(model);
        await _context.SaveChangesAsync();
        return model.Id;
    }

    public async Task DeleteTransaction(int agentId, int currencyId, int transactionId)
    {
        var transaction = await _context.AgentTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            var transactionSum = transaction.TransactionSum;
            FormattableString query =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {agentId} and currency = {currencyId} and id > {transactionId}";
            _context.AgentTransactions.Remove(transaction);

            var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
            foreach (var tr in afterTransactions) tr.Balance = tr.Balance - transactionSum;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CatalogueModel?> EditMainCatPrices(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId,
        int endCount)
    {
        var mainCat = await _context.MainCats.Include(x => x.MainCatPrices).ThenInclude(x => x.Currency)
            .Include(x => x.Producer).FirstOrDefaultAsync(x => x.Id == mainCatId);
        if (mainCat != null)
        {
            var zakNprod = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == mainCatId);
            if (zakNprod != null)
                zakNprod.BuyCount += endCount;
            else
                await _context.ZakProdCounts.AddAsync(new ZakProdCount
                {
                    MainCatId = mainCatId,
                    BuyCount = endCount,
                    SellCount = 0
                });
            if (mainCatPrices.Any())
            {
                mainCat.MainCatPrices = mainCatPrices.Select(x => new MainCatPrice
                {
                    Count = x.Count,
                    CurrencyId = x.CurrencyId,
                    Price = x.Price
                }).ToList();
                mainCat.Count = mainCatPrices.Sum(x => x.Count);
                await _context.SaveChangesAsync().ConfigureAwait(true);

                if (mainCat.MainCatPrices.Any(x => x.Currency == null))
                    return new CatalogueModel
                    {
                        UniId = mainCat.UniId,
                        MainCatId = mainCat.Id,
                        Name = mainCat.Name,
                        Count = mainCat.Count,
                        UniValue = mainCat.UniValue,
                        ProducerId = mainCat.ProducerId,
                        ProducerName = mainCat.Producer.ProducerName,
                        Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                            new CatalogueModel
                            {
                                UniId = null,
                                MainCatId = x.MainCatId,
                                PriceId = x.Id,
                                Count = x.Count,
                                Price = x.Price,
                                CurrencyId = x.CurrencyId
                            }))
                    };
                return new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId,
                            CurrencyName = x.Currency.CurrencyName
                        }))
                };
            }

            mainCat.MainCatPrices.Clear();
            mainCat.Count = 0;
            await _context.SaveChangesAsync().ConfigureAwait(true);

            return new CatalogueModel
            {
                UniId = mainCat.UniId,
                MainCatId = mainCat.Id,
                Name = mainCat.Name,
                Count = mainCat.Count,
                UniValue = mainCat.UniValue,
                ProducerId = mainCat.ProducerId,
                ProducerName = mainCat.Producer.ProducerName,
                Children = null
            };
        }

        return null;
    }

    public async Task DeleteMainCatPricesById(int mainCatId)
    {
        var prices = await _context.MainCatPrices.Where(x => x.MainCatId == mainCatId).ToListAsync();

        if (prices != null) _context.MainCatPrices.RemoveRange(prices);
        await _context.SaveChangesAsync();
    }

    public async Task EditCurrency(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds)
    {
        foreach (var item in deletedIds)
            if (item != 1 && item != 2)
            {
                var entity = await _context.Currencies.FindAsync(item);
                if (entity != null) _context.Remove(entity);
            }

        await _context.SaveChangesAsync();


        await _context.Currencies.AddRangeAsync(currencyModels.Where(x => x.Id == null).Select(x => new Currency
        {
            CanDelete = x.CanDelete,
            CurrencyName = x.CurrencyName,
            CurrencySign = x.CurrencySign,
            ToUsd = x.ToUsd
        }));
        foreach (var item in currencyModels.Where(x => x.Id != null))
            if (item.Id != 1 && item.Id != 2)
            {
                var curr = await _context.Currencies.FindAsync(item.Id);
                if (curr != null)
                {
                    curr.CurrencyName = item.CurrencyName;
                    curr.CurrencySign = item.CurrencySign;
                    curr.ToUsd = item.ToUsd;
                }
            }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllCurrencies()
    {
        _context.Currencies.RemoveRange(await _context.Currencies.Where(x => x.Id != 1 || x.Id != 2).ToListAsync());
        await _context.SaveChangesAsync();
    }

    public async Task AddNewZakupka(IEnumerable<ZakupkaAltModel> zakupka, ZakupkiModel zakMain)
    {
        foreach (var model in zakupka)
        {
            var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
            if (zakAndProd != null)
            {
                zakAndProd.BuyCount += model.Count;
            }
            else
            {
                var part = await _context.MainCats.FindAsync(model.MainCatId);
                if (part != null)
                    await _context.ZakProdCounts.AddAsync(new ZakProdCount
                    {
                        MainCatId = model.MainCatId ?? 1,
                        BuyCount = model.Count,
                        SellCount = 0
                    });
            }

            await _context.SaveChangesAsync();
        }

        await _context.ZakMainGroups.AddAsync(new ZakMainGroup
        {
            TransactionId = zakMain.TransactionId,
            Datetime = Converters.ToDateTimeSqlite(zakMain.Datetime),
            AgentId = zakMain.AgentId,
            CurrencyId = zakMain.CurrencyId,
            TotalSum = zakMain.TotalSum,
            Comment = zakMain.Comment,
            Zakupkas = zakupka.Select(x => new Zakupka
            {
                Count = x.Count,
                MainCatId = x.MainCatId,
                Price = x.Price,
                MainName = x.MainName,
                UniValue = x.UniValue
            }).ToList()
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CatalogueModel>> AddNewPricesForParts(IEnumerable<ZakupkaAltModel> parts,
        int currencyId)
    {
        var cata = new List<CatalogueModel>();
        await _context.MainCatPrices.AddRangeAsync(parts.Select(x => new MainCatPrice
        {
            Count = x.Count,
            CurrencyId = currencyId,
            MainCatId = x.MainCatId ?? default,
            Price = x.Price
        }));
        await _context.SaveChangesAsync();

        foreach (var part in parts)
        {
            var item = await _context.MainCats.FindAsync(part.MainCatId);
            if (item != null)
            {
                item.Count += part.Count;
                await _context.SaveChangesAsync();
            }

            var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == part.MainCatId);
            if (mainCat != null)
                cata.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
        }

        return cata;
    }

    public async Task DeleteZakupkaByTransactionId(int transactionId)
    {
        var transaction = await _context.AgentTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            _context.AgentTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithCountReCalc(int transactionId,
        IEnumerable<ZakupkaAltModel> zakupkaAltModels)
    {
        var uniIds = new List<CatalogueModel>();
        foreach (var item in zakupkaAltModels)
        {
            var zakNprod = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
            if (zakNprod != null)
                zakNprod.BuyCount -= item.Count;
            await _context.SaveChangesAsync();


            if (item.MainCatId != null)
            {
                var count = item.Count;
                var cataPrices = await _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId).ToListAsync();
                var cataCount = cataPrices.Sum(x => x.Count);

                if (cataCount <= count)
                {
                    var prices = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                    _context.MainCatPrices.RemoveRange(prices);
                    var partCatalogue = await _context.MainCats.FindAsync(item.MainCatId);
                    if (partCatalogue != null)
                        partCatalogue.Count = 0;
                }
                else
                {
                    count *= -1;
                    foreach (var price in cataPrices)
                    {
                        price.Count += count;
                        count = price.Count;
                        if (count >= 0)
                            break;
                    }

                    foreach (var pr in cataPrices)
                        if (pr.Count <= 0)
                        {
                            var cataPrice = await _context.MainCatPrices.FindAsync(pr.Id);
                            if (cataPrice != null)
                                _context.MainCatPrices.Remove(cataPrice);
                        }

                    cataPrices.RemoveAll(x => x.Count <= 0);
                }

                await _context.SaveChangesAsync();

                var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                    .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item.MainCatId);
                if (mainCat != null)
                {
                    mainCat.Count = await _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId)
                        .SumAsync(x => x.Count);
                    uniIds.Add(new CatalogueModel
                    {
                        UniId = mainCat.UniId,
                        MainCatId = mainCat.Id,
                        Name = mainCat.Name,
                        Count = mainCat.Count,
                        UniValue = mainCat.UniValue,
                        ProducerId = mainCat.ProducerId,
                        ProducerName = mainCat.Producer.ProducerName,
                        Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                            new CatalogueModel
                            {
                                UniId = null,
                                MainCatId = x.MainCatId,
                                PriceId = x.Id,
                                Count = x.Count,
                                Price = x.Price,
                                CurrencyId = x.CurrencyId
                            }))
                    });
                }
            }
        }

        var tr = await ReCalcTransactions(transactionId);
        if (tr != null)
            _context.AgentTransactions.Remove(tr);
        await _context.SaveChangesAsync();

        await _context.SaveChangesAsync();
        return uniIds;
    }

    public async Task<IEnumerable<CatalogueModel>> EditZakupka(IEnumerable<int> deletedIds,
        IEnumerable<ZakupkaAltModel> zakupkaAlts,
        Dictionary<int, int> lastCounts, CurrencyModel currency, string date, decimal totalSum, int transactionId,
        string comment)
    {
        var uniIds = new List<int>();

        foreach (var item in deletedIds)
        {
            var zakupkaAltItem = await _context.Zakupkas.FindAsync(item);
            if (zakupkaAltItem != null)
            {
                var zakAndProd =
                    await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == zakupkaAltItem.MainCatId);
                if (zakAndProd != null)
                    zakAndProd.BuyCount -= zakupkaAltItem.Count;
                await _context.SaveChangesAsync();


                var prices = _context.MainCatPrices.Where(x => x.MainCatId == zakupkaAltItem.MainCatId);
                var cataloguePart = await _context.MainCats.FindAsync(zakupkaAltItem.MainCatId);

                var count = zakupkaAltItem.Count;
                var pricesTotalCount = prices.Sum(x => x.Count);

                if (pricesTotalCount <= count)
                {
                    _context.MainCatPrices.RemoveRange(prices);
                    if (cataloguePart != null)
                    {
                        cataloguePart.Count = 0;
                        uniIds.Add(cataloguePart.Id);
                    }
                }
                else
                {
                    count *= -1;
                    foreach (var price in prices)
                    {
                        price.Count += count;
                        count = price.Count;
                        if (count >= 0)
                            break;
                    }

                    _context.MainCatPrices.RemoveRange(prices.Where(x => x.Count <= 0));
                    if (cataloguePart != null)
                    {
                        cataloguePart.Count = prices.Sum(x => x.Count);
                        uniIds.Add(cataloguePart.Id);
                    }
                }

                _context.Zakupkas.Remove(zakupkaAltItem);
            }
        }

        await _context.SaveChangesAsync();
        foreach (var item in zakupkaAlts)
            if (item.Id == null)
            {
                var zaknProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zaknProd != null)
                {
                    zaknProd.BuyCount += item.Count;
                }
                else
                {
                    var part = await _context.MainCats.FindAsync(item.MainCatId);
                    if (part != null)
                        await _context.ZakProdCounts.AddAsync(new ZakProdCount
                        {
                            MainCatId = item.MainCatId ?? 1,
                            BuyCount = item.Count,
                            SellCount = 0
                        });
                }


                await _context.Zakupkas.AddAsync(new Zakupka
                {
                    MainCatId = item.MainCatId,
                    MainName = item.MainName,
                    UniValue = item.UniValue,
                    Count = item.Count,
                    Price = item.Price,
                    ZakId = item.ZakupkaId
                });
                await _context.MainCatPrices.AddAsync(new MainCatPrice
                {
                    Count = item.Count,
                    CurrencyId = currency.Id ?? 0,
                    MainCatId = item.MainCatId ?? 0,
                    Price = item.Price / currency.ToUsd
                });
                var cata = await _context.MainCats.FindAsync(item.MainCatId);
                if (cata != null)
                {
                    cata.Count = item.Count;
                    uniIds.Add(cata.Id);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var cataPrice = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                var zakupkaAlt = await _context.Zakupkas.FirstOrDefaultAsync(x => x.Id == item.Id);
                var mainCat = await _context.MainCats.FindAsync(item.MainCatId);

                var totalCount = cataPrice.Sum(x => x.Count);
                var prevCount = lastCounts[item.MainCatId ?? 0];
                lastCounts[item.MainCatId ?? 0] -= prevCount;
                var currCount = item.Count;
                var count = currCount - prevCount;

                if (zakupkaAlt != null)
                {
                    zakupkaAlt.Count = item.Count;
                    zakupkaAlt.Price = item.Price;
                }

                var zakAndProd =
                    await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zakAndProd != null) zakAndProd.BuyCount += count;


                await _context.SaveChangesAsync();

                if (totalCount <= 0)
                {
                    if (count <= 0)
                    {
                        _context.MainCatPrices.RemoveRange(cataPrice);
                        if (mainCat != null)
                        {
                            mainCat.Count = 0;
                            uniIds.Add(mainCat.Id);
                        }
                    }
                    else
                    {
                        await _context.MainCatPrices.AddAsync(new MainCatPrice
                        {
                            Count = count,
                            CurrencyId = currency.Id ?? 1,
                            MainCatId = item.MainCatId ?? 0,
                            Price = item.Price / currency.ToUsd
                        });
                        if (mainCat != null)
                        {
                            mainCat.Count = count;
                            uniIds.Add(mainCat.Id);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    var price = cataPrice.FirstOrDefault();
                    if (price != null)
                    {
                        if (count < 0)
                        {
                            foreach (var p in cataPrice)
                            {
                                p.Count += count;
                                count = p.Count;
                                if (p.Count <= 0)
                                    _context.MainCatPrices.Remove(p);
                                if (count >= 0)
                                    break;
                            }

                            if (mainCat != null)
                            {
                                mainCat.Count = _context.MainCatPrices
                                    .Where(x => x.MainCatId == item.MainCatId && x.Count > 0).Sum(x => x.Count);
                                uniIds.Add(mainCat.Id);
                            }
                        }
                        else
                        {
                            price.Count += count;
                            if (mainCat != null)
                            {
                                mainCat.Count += count;
                                uniIds.Add(mainCat.Id);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }

        var mainGroup = await _context.ZakMainGroups.FindAsync(zakupkaAlts.First().ZakupkaId);
        if (mainGroup != null)
        {
            mainGroup.TotalSum = totalSum;
            mainGroup.Datetime = date;
            mainGroup.CurrencyId = currency.Id ?? 2;
            mainGroup.Comment = comment;
        }

        var tr = await _context.AgentTransactions.FindAsync(transactionId);
        if (tr != null)
        {
            var diff = -1 * (tr.TransactionSum + totalSum);
            var transaction = await ReCalcTransactions(transactionId, diff);
        }


        await _context.SaveChangesAsync();

        var catas = new List<CatalogueModel>();
        foreach (var item in uniIds.Distinct().ToList())
        {
            var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item);
            if (mainCat != null)
                catas.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
        }

        return catas;
    }

    public async Task<IEnumerable<CatalogueModel>> EditProdaja(IEnumerable<Tuple<int, decimal>> deletedIds,
        IEnumerable<ProdajaAltModel> prodajaAltModels,
        Dictionary<int, int> lastCounts, CurrencyModel currency, string date, decimal totalSum, int transactionId,
        string comment)
    {
        var uniIds = new List<int>();

        foreach (var item in deletedIds)
        {
            var prodajaItem = await _context.Prodajas.FindAsync(item.Item1);
            if (prodajaItem != null)
            {
                var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.Item1);
                if (zakAndProd != null)
                    zakAndProd.SellCount -= prodajaItem.Count;

                await _context.SaveChangesAsync();

                var prices =
                    await _context.MainCatPrices.FirstOrDefaultAsync(x => x.MainCatId == prodajaItem.MainCatId);
                var cataloguePart = await _context.MainCats.FindAsync(prodajaItem.MainCatId);

                if (cataloguePart != null)
                {
                    if (prices != null)
                    {
                        prices.Count += prodajaItem.Count;
                        cataloguePart.Count += prodajaItem.Count;
                        uniIds.Add(cataloguePart.Id);
                    }
                    else
                    {
                        cataloguePart.Count += prodajaItem.Count;
                        uniIds.Add(cataloguePart.Id);
                        await _context.MainCatPrices.AddAsync(new MainCatPrice
                        {
                            CurrencyId = prodajaItem.CurrencyId,
                            MainCatId = cataloguePart.Id,
                            Price = prodajaItem.InitialPrice
                        });
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        var prodCount = prodajaAltModels.Where(x => x.Id != null).ToList();
        foreach (var item in prodajaAltModels)
            if (item.Id == null)
            {
                var zaknProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zaknProd != null)
                {
                    zaknProd.SellCount += item.Count;
                }
                else
                {
                    var part = await _context.MainCats.FindAsync(item.MainCatId);
                    if (part != null) ;
                    {
                        await _context.ZakProdCounts.AddAsync(new ZakProdCount
                        {
                            MainCatId = item.MainCatId ?? 1,
                            BuyCount = 0,
                            SellCount = item.Count
                        });
                    }
                }


                await _context.Prodajas.AddAsync(new Prodaja
                {
                    MainCatId = item.MainCatId,
                    MainName = item.MainName,
                    UniValue = item.UniValue,
                    Count = item.Count,
                    Price = item.Price,
                    ProdajaId = item.ProdajaId
                });
                await _context.SaveChangesAsync();

                var prices = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                if (prices.Any())
                {
                    var count = item.Count * -1;
                    foreach (var pr in prices)
                    {
                        pr.Count += count;
                        count = pr.Count;
                        if (count >= 0)
                            break;
                    }

                    foreach (var p in prices)
                        if (item.Count <= 0)
                            _context.MainCatPrices.Remove(p);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var cataPrice = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                var prodajaAlt = await _context.Prodajas.FindAsync(item.Id);
                var mainCat = await _context.MainCats.FindAsync(item.MainCatId);

                var prevCount = lastCounts[item.MainCatId ?? 0] / prodCount.Count(x => x.MainCatId == item.MainCatId);
                lastCounts[item.MainCatId ?? 0] -= prevCount;
                var currCount = item.Count;
                var count = currCount - prevCount;

                var zakAndProd =
                    await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zakAndProd != null)
                    zakAndProd.SellCount += count;

                await _context.SaveChangesAsync();

                if (prodajaAlt != null)
                {
                    prodajaAlt.Count = item.Count;
                    prodajaAlt.Price = item.Price;
                }

                if (cataPrice.Any())
                {
                    if (count != 0)
                    {
                        var c = count * -1;
                        foreach (var pr in cataPrice)
                        {
                            pr.Count += c;
                            c = pr.Count;
                            if (pr.Count <= 0)
                                _context.MainCatPrices.Remove(pr);

                            if (c >= 0)
                                break;
                        }

                        await _context.SaveChangesAsync();

                        if (mainCat != null)
                        {
                            mainCat.Count = await cataPrice.SumAsync(x => x.Count);
                            uniIds.Add(mainCat.Id);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (count < 0)
                        if (mainCat != null)
                        {
                            await _context.MainCatPrices.AddAsync(new MainCatPrice
                            {
                                MainCatId = mainCat.Id,
                                Count = count * -1,
                                Price = item.Price / currency.ToUsd,
                                CurrencyId = currency.Id ?? 2
                            });
                            mainCat.Count += count * -1;
                            uniIds.Add(mainCat.Id);
                            await _context.SaveChangesAsync();
                        }
                }
            }

        var mainGroup = await _context.ProdMainGroups.FindAsync(prodajaAltModels.First().ProdajaId);
        if (mainGroup != null)
        {
            mainGroup.TotalSum = totalSum;
            mainGroup.Datetime = date;
            mainGroup.CurrencyId = currency.Id ?? 0;
            mainGroup.Comment = comment;
        }

        var tr = await _context.AgentTransactions.FindAsync(transactionId);
        if (tr != null)
        {
            var diff = -1 * (tr.TransactionSum - totalSum);
            var transaction = await ReCalcTransactions(transactionId, diff);
        }

        await _context.SaveChangesAsync();
        var catas = new List<CatalogueModel>();
        foreach (var item in uniIds.Distinct().ToList())
        {
            var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item);
            if (mainCat != null)
                catas.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
        }

        return catas;
    }

    public async Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models,
        ProdajaModel mainModel)
    {
        var catas = new List<CatalogueModel>();
        foreach (var model in models)
        {
            var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
            if (zakAndProd != null)
            {
                zakAndProd.SellCount += model.Count;
            }
            else
            {
                var part = await _context.MainCats.FindAsync(model.MainCatId);
                if (part != null)
                    await _context.ZakProdCounts.AddAsync(new ZakProdCount
                    {
                        MainCatId = model.MainCatId ?? 1,
                        BuyCount = 0,
                        SellCount = model.Count
                    });
            }

            await _context.SaveChangesAsync();

            var count = model.Count * -1;
            var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
            var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);

            foreach (var p in prices)
            {
                p.Count += count;
                count = p.Count;
                if (p.Count <= 0)
                    _context.MainCatPrices.Remove(p);
                if (count >= 0)
                    break;
            }

            await _context.SaveChangesAsync();

            if (mainCat != null)
            {
                mainCat.Count -= model.Count;
                catas.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
            }
        }

        await _context.ProdMainGroups.AddAsync(new ProdMainGroup
        {
            AgentId = mainModel.AgentId,
            CurrencyId = mainModel.CurrencyId,
            Datetime = mainModel.Datetime,
            Comment = mainModel.Comment,
            TransactionId = mainModel.TransactionId,
            TotalSum = mainModel.TotalSum,
            Prodajas = models.Select(x => new Prodaja
            {
                Count = x.Count,
                MainCatId = x.MainCatId,
                MainName = x.MainName,
                Price = x.Price,
                UniValue = x.UniValue,
                InitialPrice = x.InitialPrice,
                CurrencyId = x.CurrencyInitialId
            }).ToList()
        });

        await _context.SaveChangesAsync();
        return catas;
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteProdajaCountReCalc(int transactionId,
        IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId)
    {
        var catas = new List<CatalogueModel>();

        foreach (var model in prodajaAltModels)
            if (model.MainCatId != null)
            {
                var zakNprod = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
                if (zakNprod != null)
                    zakNprod.SellCount -= model.Count;
                await _context.SaveChangesAsync();

                var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
                var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                    .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);
                if (prices.Any())
                {
                    prices.First().Count += model.Count;
                }
                else
                {
                    var currency = await _context.Currencies.FindAsync(currencyId);

                    if (currency != null)
                        await _context.MainCatPrices.AddAsync(new MainCatPrice
                        {
                            MainCatId = model.MainCatId ?? 5923,
                            Count = model.Count,
                            CurrencyId = model.CurrencyInitialId,
                            Price = model.InitialPrice
                        });
                }

                await _context.SaveChangesAsync();

                if (mainCat != null)
                {
                    mainCat.Count += model.Count;
                    catas.Add(new CatalogueModel
                    {
                        UniId = mainCat.UniId,
                        MainCatId = mainCat.Id,
                        Name = mainCat.Name,
                        Count = mainCat.Count,
                        UniValue = mainCat.UniValue,
                        ProducerId = mainCat.ProducerId,
                        ProducerName = mainCat.Producer.ProducerName,
                        Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                            new CatalogueModel
                            {
                                UniId = null,
                                MainCatId = x.MainCatId,
                                PriceId = x.Id,
                                Count = x.Count,
                                Price = x.Price,
                                CurrencyId = x.CurrencyId
                            }))
                    });
                }
            }

        var transaction = await ReCalcTransactions(transactionId);
        if (transaction != null)
            _context.AgentTransactions.Remove(transaction);

        await _context.SaveChangesAsync();
        return catas;
    }

    public async Task<int?> CheckCanDeleteProdaja(int? mainCatId)
    {
        if (mainCatId == null)
            return null;

        var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == mainCatId);

        if (zakAndProd != null) return zakAndProd.BuyCount - zakAndProd.SellCount;

        var zakCount = await _context.Zakupkas.Where(x => x.MainCatId == mainCatId).SumAsync(x => x.Count);
        var prodCount = await _context.Prodajas.Where(x => x.MainCatId == mainCatId).SumAsync(x => x.Count);
        var mainCat = await _context.MainCats.FindAsync(mainCatId);
        if (mainCat != null)
        {
            await _context.ZakProdCounts.AddAsync(new ZakProdCount
            {
                MainCatId = mainCatId ?? 1,
                BuyCount = zakCount,
                SellCount = prodCount
            });
            await _context.SaveChangesAsync();

            var differ = zakCount - prodCount;
            return differ;
        }

        return null;
    }

    private async Task<AgentTransaction?> ReCalcTransactions(int transactionId)
    {
        var transaction = await _context.AgentTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            var transactionSum = transaction.TransactionSum;
            FormattableString query =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency} and id > {transactionId}";

            var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
            foreach (var tr in afterTransactions) tr.Balance = tr.Balance - transactionSum;
            await _context.SaveChangesAsync();
            return transaction;
        }

        return transaction;
    }

    private async Task<AgentTransaction?> ReCalcTransactions(int transactionId, decimal diff)
    {
        var transaction = await _context.AgentTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            transaction.TransactionSum += diff;
            transaction.Balance += diff;
            FormattableString query =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency} and id > {transactionId}";

            var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
            foreach (var tr in afterTransactions)
                tr.Balance = tr.Balance + diff;
            await _context.SaveChangesAsync();
            return transaction;
        }

        return transaction;
    }
}