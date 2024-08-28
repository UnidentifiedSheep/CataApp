using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Views.DialogueWindows;
using DataBase.Data;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SQLitePCL;
using Action = DataBase.Data.Action;

namespace CatalogueAvalonia.Services.DataBaseAction;

public class DataBaseAction : IDataBaseAction
{
    enum ActionsEnum
    {
        Write = 0,
        Update = 1,
        Delete = 2
    }

    private readonly DataContext _context;

    public DataBaseAction(DataContext dataContext)
    {
        _context = dataContext;
    }

    public async Task DeleteMainNameById(int? id)
    {
        var el = await _context.MainNames.FindAsync(id);
        string mainName = "";
        if (el != null)
        {
            mainName = el.Name;
            _context.MainNames.Remove(el);
            await _context.SaveChangesAsync();
            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Delete,
                Values = mainName,
                Description = $"Удалено id={id}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
        }
    }

    public async Task DeleteFromMainCatById(int? id)
    {
        var el = await _context.MainCats.FindAsync(id);
        if (el != null)
        {
            var uniValue = el.UniValue;
            var uniId = el.UniId;

            _context.MainCats.Remove(el);

            await _context.SaveChangesAsync();
            await ReCallcMainCount(el.UniId);

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Delete,
                Values = uniValue,
                Description = $"Удалено id={id}, uni_id={uniId}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
        }
    }

    public async Task EditCatalogue(CatalogueModel catalogue, List<int> deleteIds)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        
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
            var name = mainName.Name;
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
            await ReCallcMainCount(mainName.UniId);
            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Update,
                Values = name,
                Description = $"Редактировано uni_id={uniId}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
        }

        await dbTransaction.CommitAsync();
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

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Write,
                Values = model.Name,
                Description = $"Добавлено uni_id={model.UniId}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });

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
            model.OverPr = agentModel.OverPrice ?? 0;

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Update,
                Values = model.Name,
                Description = $"Редактировано id={model.Id}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AgentModel> AddNewAgent(string name, int isZak)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        var model = new Agent { Name = name, IsZak = isZak };
        await _context.Agents.AddAsync(model);
        await _context.SaveChangesAsync();
        
        foreach (var curr in _context.Currencies)
        {
            var time = DateTime.Now.ToString("HH:mm:ss").Replace(":", "");
            await _context.AgentTransactions.AddAsync(new AgentTransaction
            {
                AgentId = model.Id,
                Balance = 0,
                Currency = curr.Id,
                Time = time.Length == 6 ? time : "0" + time,
                TransactionDatatime = Converters.ToDateTimeSqlite(DateTime.Now.ToString("dd.MM.yyyy")),
                TransactionStatus = 3,
                TransactionSum = 0
            });
            await _context.AgentBalances.AddAsync(new AgentBalance
            {
                AgentId = model.Id,
                CurrencyId = curr.Id,
                Balance = 0m
            });
            await _context.SaveChangesAsync();
        }
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Write,
            Values = model.Name,
            Description = $"Добавлено id={model.Id}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        await dbTransaction.CommitAsync();
        return new AgentModel
        {
            Id = model.Id, IsZak = model.IsZak, Name = model.Name, OverPrice = model.OverPr,
            OverPriceText = model.OverPr.ToString()
        };
    }

    public async Task DeleteAgent(int id)
    {
        var model = await _context.Agents.FindAsync(id);
        if (model != null)
        {
            var name = model.Name;
            _context.Agents.Remove(model);

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Delete,
                Values = name,
                Description = $"Удалено id={id}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
            await _context.SaveChangesAsync();
        }
    }
    
    private async Task<decimal> PrevBalance(int agentId, int currencyId, string date, string time)
    {
        List<AgentTransaction> endList = new();
        FormattableString sameDate =
            $"SELECT * from agent_transactions where {date} = transaction_datatime and transaction_status != 3 and agent_id = {agentId} and currency = {currencyId} and time <= {time} order by transaction_datatime desc , time desc";
        FormattableString otherDays =
            $"SELECT * from agent_transactions where {date} > transaction_datatime and transaction_status != 3 and agent_id = {agentId} and currency = {currencyId} order by transaction_datatime desc , time desc limit 1";
        endList.AddRange(await _context.AgentTransactions.FromSql(sameDate).ToListAsync());
        
        if (endList.Count != 0) return endList.First().Balance;
        
        endList.AddRange(await _context.AgentTransactions.FromSql(otherDays).ToListAsync());
        
        var balance = endList.FirstOrDefault();
        return balance?.Balance ?? 0m;
    }

    private async Task<int> AddNewTransactionNoTracking(AgentTransactionModel agentTransaction, string time)
    {
        var transaction = new AgentTransaction
        {
            AgentId = agentTransaction.AgentId,
            TransactionDatatime = Converters.ToDateTimeSqlite(agentTransaction.TransactionDatatime),
            TransactionStatus = agentTransaction.TransactionStatus,
            TransactionSum = agentTransaction.TransactionSum,
            Currency = agentTransaction.CurrencyId,
            Balance = agentTransaction.Balance,
            Time = time.Length == 6 ? time : "0" + time,
        };
        transaction.Balance = await PrevBalance(transaction.AgentId, transaction.Currency, transaction.TransactionDatatime, transaction.Time) + transaction.TransactionSum;
        var agent = await _context.AgentBalances.FirstOrDefaultAsync(x =>
            x.AgentId == transaction.AgentId && x.CurrencyId == transaction.Currency);
        agent!.Balance += transaction.TransactionSum;
        await _context.AgentTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        FormattableString query =
            $"SELECT * from agent_transactions where {transaction.TransactionDatatime} <= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency}";
        var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
        afterTransactions.Remove(transaction);
        var items = afterTransactions.Where(x =>
            x.TransactionDatatime == transaction.TransactionDatatime &&
            Convert.ToInt32(x.Time) <= Convert.ToInt32(transaction.Time)).ToList();
        afterTransactions.Remove(items);
        foreach (var tr in afterTransactions)
            tr.Balance += transaction.TransactionSum;
        
        await _context.SaveChangesAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Write,
            Values = transaction.TransactionSum.ToString(),
            Description =
                $"Новая транзакция id={transaction.Id}, agent_id={transaction.AgentId}, transaction_status={transaction.TransactionStatus}",
            Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return transaction.Id;
    }

    public async Task<int> AddNewTransaction(AgentTransactionModel agentTransaction)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        var time = DateTime.Now.ToString("HH:mm:ss").Replace(":", "");
        var transaction = new AgentTransaction
        {
            AgentId = agentTransaction.AgentId,
            TransactionDatatime = Converters.ToDateTimeSqlite(agentTransaction.TransactionDatatime),
            TransactionStatus = agentTransaction.TransactionStatus,
            TransactionSum = agentTransaction.TransactionSum,
            Currency = agentTransaction.CurrencyId,
            Balance = agentTransaction.Balance,
            Time = time.Length == 6 ? time : "0" + time,
        };
        transaction.Balance = await PrevBalance(transaction.AgentId, transaction.Currency, transaction.TransactionDatatime, transaction.Time) + transaction.TransactionSum;
        var agent = await _context.AgentBalances.FirstOrDefaultAsync(x =>
            x.AgentId == transaction.AgentId && x.CurrencyId == transaction.Currency);
        agent!.Balance += transaction.TransactionSum;
        await _context.AgentTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        FormattableString query =
            $"SELECT * from agent_transactions where {transaction.TransactionDatatime} <= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency}";
        var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
        afterTransactions.Remove(transaction);
        var items = afterTransactions.Where(x =>
            x.TransactionDatatime == transaction.TransactionDatatime &&
            Convert.ToInt32(x.Time) <= Convert.ToInt32(transaction.Time)).ToList();
        afterTransactions.Remove(items);
        foreach (var tr in afterTransactions)
            tr.Balance += transaction.TransactionSum;
        
        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Write,
            Values = transaction.TransactionSum.ToString(),
            Description =
                $"Новая транзакция id={transaction.Id}, agent_id={transaction.AgentId}, transaction_status={transaction.TransactionStatus}",
            Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return transaction.Id;
    }

    public async Task DeleteTransaction(int agentId, int currencyId, int transactionId)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        
        var transaction = await _context.AgentTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            var transactionSum = transaction.TransactionSum;
            var agent = await _context.AgentBalances.FirstOrDefaultAsync(x =>
                x.AgentId == transaction.AgentId && x.CurrencyId == transaction.Currency);
            agent!.Balance -= transaction.TransactionSum;
            
            FormattableString query =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} <= transaction_datatime and agent_id = {agentId} and currency = {currencyId}";
            _context.AgentTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
            var items = afterTransactions.Where(x =>
                x.TransactionDatatime == transaction.TransactionDatatime &&
                Convert.ToInt32(x.Time) <= Convert.ToInt32(transaction.Time)).ToList();
            afterTransactions.Remove(items);
            foreach (var tr in afterTransactions) tr.Balance -= transactionSum;

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Delete,
                Values = transaction.Id.ToString(),
                Description =
                    $"Удалено id={transaction.Id}, transaction_sum={transaction.TransactionSum}, transaction_datetime={transaction.TransactionDatatime}",
                Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
            await _context.SaveChangesAsync();
        }

        await dbTransaction.CommitAsync();
    }

    public async Task<CatalogueModel?> EditMainCatPrices(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId,
        int endCount)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

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
                    Count = x.Count ?? 0,
                    CurrencyId = x.CurrencyId,
                    Price = x.Price ?? 0
                }).ToList();
                mainCat.Count = mainCatPrices.Sum(x => x.Count ?? 0);
                await _context.SaveChangesAsync().ConfigureAwait(true);


                var mainName = await _context.MainNames.Include(x => x.MainCats)
                    .FirstOrDefaultAsync(x => x.UniId == mainCat.UniId);
                if (mainName != null)
                {
                    mainName.Count = mainName.MainCats.Sum(x => x.Count);
                }

                await transaction.CommitAsync();

                await AddRecordToLog(new Action
                {
                    Action1 = (int)ActionsEnum.Update,
                    Values = mainCatId.ToString(),
                    Description = $"Редактировано цены id={mainCatId}, ", Seen = 0,
                    Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                    Time = DateTime.Now.ToShortTimeString()
                });

                return new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    RowColor = mainCat.RowColor,
                    TextColor = mainCat.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            Name = "  Цена:",
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                };
            }

            mainCat.MainCatPrices.Clear();
            mainCat.Count = 0;

            var main = await _context.MainNames.Include(x => x.MainCats)
                .FirstOrDefaultAsync(x => x.UniId == mainCat.UniId);
            if (main != null)
            {
                main.Count = main.MainCats.Sum(x => x.Count);
            }

            await transaction.CommitAsync();

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Update,
                Values = mainCatId.ToString(),
                Description = $"Редактировано цены id={mainCatId}, ", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });

            return new CatalogueModel
            {
                UniId = mainCat.UniId,
                RowColor = mainCat.RowColor,
                TextColor = mainCat.TextColor,
                MainCatId = mainCat.Id,
                Name = mainCat.Name,
                Count = mainCat.Count,
                UniValue = mainCat.UniValue,
                ProducerId = mainCat.ProducerId,
                ProducerName = mainCat.Producer.ProducerName,
                Children = null
            };
        }

        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Update,
            Values = mainCatId.ToString(),
            Description = $"Редактировано цены id={mainCatId}, ", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });

        await transaction.CommitAsync();
        return null;
    }

    public async Task DeleteMainCatPricesById(int mainCatId)
    {
        List<MainCatPrice>? prices = await _context.MainCatPrices.Where(x => x.MainCatId == mainCatId).ToListAsync();
        if (prices != null) _context.MainCatPrices.RemoveRange(prices);
        var mainCat = await _context.MainCats.FindAsync(mainCatId);
        if (mainCat!=null) await ReCallcMainCount(mainCat.UniId);
        
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Delete,
            Values = mainCatId.ToString(),
            Description = $"Удалено цены id={mainCatId}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
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

    public async Task<IEnumerable<CatalogueModel>> AddNewZakupka(IEnumerable<ZakupkaAltModel> zakupka, ZakupkiModel zakMain,AgentTransactionModel initTransaction, AgentTransactionModel agentPayment,
        IEnumerable<ZakupkaAltModel> catas,
        int currencyId)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();

        var time = Convert.ToInt32(DateTime.Now.ToString("HH:mm:ss").Replace(":", ""));
        time = time % 100 == 59 ? time - 1 : time;
        var transaction = await AddNewTransactionNoTracking(initTransaction, time.ToString());
        zakMain.TransactionId = transaction;
        if (agentPayment.AgentId != 0)
            await AddNewTransactionNoTracking(agentPayment, (time + 1).ToString());
        
        foreach (var model in zakupka)
        {
            var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
            if (zakAndProd != null)
            {
                zakAndProd.BuyCount += model.Count ?? 0;
            }
            else
            {
                var part = await _context.MainCats.FindAsync(model.MainCatId);
                if (part != null)
                    await _context.ZakProdCounts.AddAsync(new ZakProdCount
                    {
                        MainCatId = model.MainCatId ?? 1,
                        BuyCount = model.Count ?? 0,
                        SellCount = 0
                    });
            }

            await _context.SaveChangesAsync();
        }

        var a = new ZakMainGroup
        {
            TransactionId = zakMain.TransactionId,
            Datetime = Converters.ToDateTimeSqlite(zakMain.Datetime),
            AgentId = zakMain.AgentId,
            CurrencyId = zakMain.CurrencyId,
            TotalSum = zakMain.TotalSum,
            Comment = zakMain.Comment,
            Zakupkas = zakupka.Select(x => new Zakupka
            {
                Count = x.Count ?? 0,
                MainCatId = x.MainCatId,
                Price = x.Price ?? 0,
                MainName = x.MainName,
                UniValue = x.UniValue
            }).ToList()
        };
        await _context.ZakMainGroups.AddAsync(a);

        var cat = await AddNewPricesForParts(catas, currencyId);
        await dbTransaction.CommitAsync();

        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Write,
            Values = a.Id.ToString(),
            Description = $"Добавлена закупка id={a.Id}, agent_id={a.AgentId}, total_sum={a.TotalSum}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return cat;
    }

    public async Task<IEnumerable<CatalogueModel>> AddNewPricesForParts(IEnumerable<ZakupkaAltModel> parts,
        int currencyId)
    {
        var cata = new List<CatalogueModel>();
        await _context.MainCatPrices.AddRangeAsync(parts.Select(x => new MainCatPrice
        {
            Count = x.Count ?? 0,
            CurrencyId = currencyId,
            MainCatId = x.MainCatId ?? default,
            Price = x.Price ?? 0
        }));
        await _context.SaveChangesAsync();

        foreach (var part in parts)
        {
            var item = await _context.MainCats.FindAsync(part.MainCatId);
            if (item != null)
            {
                item.Count += part.Count ?? 0;
                await _context.SaveChangesAsync();
            }

            var mainCat = await _context.MainCats.Include(x => x.Uni).Include(x => x.Producer)
                .Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == part.MainCatId);
            if (mainCat != null)
            {
                var mainName = await _context.MainNames.Include(x => x.MainCats)
                    .FirstOrDefaultAsync(x => x.UniId == mainCat.UniId);
                if (mainName != null)
                    mainName.Count = mainName.MainCats.Sum(x => x.Count);

                cata.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    RowColor = mainCat.RowColor,
                    TextColor = mainCat.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            Name = "  Цена:",
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
            }

            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Write,
                Values = mainCat.UniValue,
                Description = $"Добавлена цена id={mainCat.Id}, uni_value={mainCat.UniValue}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
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
            await AddRecordToLog(new Action
            {
                Action1 = (int)ActionsEnum.Delete,
                Values = transactionId.ToString(),
                Description = $"Удалена закупка transaction_id={transactionId}", Seen = 0,
                Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
                Time = DateTime.Now.ToShortTimeString()
            });
        }
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithCountReCalc(int transactionId,
        IEnumerable<ZakupkaAltModel> zakupkaAltModels)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var uniIds = new List<CatalogueModel>();
        foreach (var item in zakupkaAltModels)
        {
            var zakNprod = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
            if (zakNprod != null)
                zakNprod.BuyCount -= item.Count ?? 0;
            await _context.SaveChangesAsync();


            if (item.MainCatId != null)
            {
                var count = item.Count;
                var cataPrices = await _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId).ToListAsync();
                var cataCount = cataPrices.Sum(x => x.Count);

                if (cataCount <= count)
                {
                    var prices = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                    foreach (var price in prices)
                    {
                        price.Count = 0;
                    }

                    var partCatalogue = await _context.MainCats.FindAsync(item.MainCatId);
                    if (partCatalogue != null)
                        partCatalogue.Count = 0;
                }
                else
                {
                    count *= -1;
                    foreach (var price in cataPrices)
                    {
                        price.Count += count ?? 0;
                        count = price.Count;
                        if (count >= 0)
                            break;
                    }

                    foreach (var pr in cataPrices)
                        if (pr.Count < 0)
                        {
                            var cataPrice = await _context.MainCatPrices.FindAsync(pr.Id);
                            if (cataPrice != null)
                                cataPrice.Count = 0;
                        }

                    cataPrices.RemoveAll(x => x.Count < 0);
                }

                await _context.SaveChangesAsync();

                var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                    .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item.MainCatId);
                if (mainCat != null)
                {
                    mainCat.Count = await _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId)
                        .SumAsync(x => x.Count);
                    var mainName = await _context.MainNames.Include(x => x.MainCats)
                        .FirstOrDefaultAsync(x => x.UniId == mainCat.UniId);
                    if (mainName != null)
                    {
                        mainName.Count = mainName.MainCats.Sum(x => x.Count);
                    }

                    uniIds.Add(new CatalogueModel
                    {
                        UniId = mainCat.UniId,
                        MainCatId = mainCat.Id,
                        Name = mainCat.Name,
                        Count = mainCat.Count,
                        UniValue = mainCat.UniValue,
                        ProducerId = mainCat.ProducerId,
                        ProducerName = mainCat.Producer.ProducerName,
                        RowColor = mainCat.RowColor,
                        TextColor = mainCat.TextColor,
                        Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                            new CatalogueModel
                            {
                                Name = "  Цена:",
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

        await transaction.CommitAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Delete,
            Values = transactionId.ToString(),
            Description = $"Удалена закупка transaction_id={transactionId}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return uniIds;
    }
    private async Task ReCallcMainCount(int uniId)
    {
        var mainName = await _context.MainNames.Include(x => x.MainCats).FirstOrDefaultAsync(x => x.UniId == uniId);
        if (mainName != null)
            mainName.Count = mainName.MainCats.Sum(x => x.Count);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CatalogueModel>> EditZakupka(IEnumerable<Tuple<int, int>> deletedIds,
        IEnumerable<ZakupkaAltModel> zakupkaAlts,
        Dictionary<int, int> lastCounts, CurrencyModel currency, string date, decimal totalSum, int transactionId,
        string comment)
    {
        await using var dBTransaction = await _context.Database.BeginTransactionAsync();
        var uniIds = new List<int>();

        foreach (var item in deletedIds)
        {
            var zakupkaAltItem = await _context.Zakupkas.FindAsync(item.Item1);
            if (zakupkaAltItem != null)
            {
                var zakAndProd =
                    await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == zakupkaAltItem.MainCatId);
                if (zakAndProd != null)
                {
                    zakAndProd.BuyCount -= zakupkaAltItem.Count;
                    if (lastCounts.ContainsKey(item.Item2))
                    {
                        lastCounts[item.Item2] -= zakupkaAltItem.Count;
                    }
                }

                await _context.SaveChangesAsync();


                var prices = _context.MainCatPrices.Where(x => x.MainCatId == zakupkaAltItem.MainCatId);
                var cataloguePart = await _context.MainCats.FindAsync(zakupkaAltItem.MainCatId);

                var count = zakupkaAltItem.Count;
                var pricesTotalCount = prices.Sum(x => x.Count);

                if (pricesTotalCount <= count)
                {
                    foreach (var price in prices)
                    {
                        price.Count = 0;
                    }

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
                        if (price.Count < 0)
                            price.Count = 0;
                        if (count >= 0)
                            break;
                    }

                    await _context.SaveChangesAsync();
                    if (cataloguePart != null)
                    {
                        cataloguePart.Count = prices.Sum(x => x.Count);
                        uniIds.Add(cataloguePart.Id);
                        await _context.SaveChangesAsync();
                        await ReCallcMainCount(cataloguePart.UniId);
                    }
                }


                _context.Zakupkas.Remove(zakupkaAltItem);
                await _context.SaveChangesAsync();
            }
        }

        await _context.SaveChangesAsync();
        foreach (var item in zakupkaAlts)
            if (item.Id == null)
            {
                var zaknProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zaknProd != null)
                {
                    zaknProd.BuyCount += item.Count ?? 0;
                }
                else
                {
                    var part = await _context.MainCats.FindAsync(item.MainCatId);
                    if (part != null)
                    {
                        await _context.ZakProdCounts.AddAsync(new ZakProdCount
                        {
                            MainCatId = item.MainCatId ?? 1,
                            BuyCount = item.Count ?? 0,
                            SellCount = 0
                        });
                    }
                }


                await _context.Zakupkas.AddAsync(new Zakupka
                {
                    MainCatId = item.MainCatId,
                    MainName = item.MainName,
                    UniValue = item.UniValue,
                    Count = item.Count ?? 0,
                    Price = item.Price ?? 0,
                    ZakId = item.ZakupkaId
                });
                await _context.MainCatPrices.AddAsync(new MainCatPrice
                {
                    Count = item.Count ?? 0,
                    CurrencyId = currency.Id ?? 0,
                    MainCatId = item.MainCatId ?? 0,
                    Price = (item.Price ?? 0) / currency.ToUsd
                });
                await _context.SaveChangesAsync();
                var cata = await _context.MainCats.FindAsync(item.MainCatId);
                if (cata != null)
                {
                    cata.Count = await _context.MainCatPrices.Where(x => x.MainCatId == cata.Id).SumAsync(x => x.Count);
                    uniIds.Add(cata.Id);
                    await ReCallcMainCount(cata.UniId);
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
                    zakupkaAlt.Count = item.Count ?? 0;
                    zakupkaAlt.Price = item.Price ?? 0;
                }

                var zakAndProd =
                    await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                if (zakAndProd != null) zakAndProd.BuyCount += count ?? 0;


                await _context.SaveChangesAsync();

                if (totalCount <= 0)
                {
                    if (count <= 0)
                    {
                        foreach (var price in cataPrice)
                            price.Count = 0;

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
                            Count = count ?? 0,
                            CurrencyId = currency.Id ?? 1,
                            MainCatId = item.MainCatId ?? 0,
                            Price = (item.Price ?? 0) / currency.ToUsd
                        });
                        if (mainCat != null)
                        {
                            mainCat.Count = await _context.MainCatPrices.Where(x => x.MainCatId == mainCat.Id)
                                .SumAsync(x => x.Count);
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
                                p.Count += count ?? 0;
                                count = p.Count;
                                if (p.Count < 0)
                                    p.Count = 0;
                                if (count >= 0)
                                    break;
                            }

                            await _context.SaveChangesAsync();
                            if (mainCat != null)
                            {
                                mainCat.Count = _context.MainCatPrices
                                    .Where(x => x.MainCatId == item.MainCatId && x.Count > 0).Sum(x => x.Count);
                                uniIds.Add(mainCat.Id);
                            }
                        }
                        else
                        {
                            price.Count += count ?? 0;
                            if (mainCat != null)
                            {
                                mainCat.Count += count ?? 0;
                                uniIds.Add(mainCat.Id);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                if (mainCat != null)
                {
                    await ReCallcMainCount(mainCat.UniId);
                }
            }

        var mainGroup = await _context.ZakMainGroups.FindAsync(zakupkaAlts.FirstOrDefault().ZakupkaId);
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
                    RowColor = mainCat.RowColor,
                    TextColor = mainCat.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            Name = "  Цена:",
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
        }

        await dBTransaction.CommitAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Update,
            Values = transactionId.ToString(),
            Description = $"Редактирована закупка transaction_id={transactionId}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return catas;
    }

    public async Task<IEnumerable<CatalogueModel>> EditProdaja(IEnumerable<Tuple<int, decimal>> deletedIds,
        IEnumerable<ProdajaAltModel> prodajaAltModels,
        Dictionary<int, int> lastCounts, CurrencyModel currency, string date, decimal totalSum, int transactionId,
        string comment)
    {
        var prodajaMain = await _context.ProdMainGroups.FirstAsync(x => x.TransactionId == transactionId);
        var prodajaMainId = prodajaMain.Id;
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
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

                    await ReCallcMainCount(cataloguePart.UniId);
                }

                _context.Prodajas.Remove(prodajaItem);
                await _context.SaveChangesAsync();
            }
        }

        await _context.SaveChangesAsync();
        var prodCount = prodajaAltModels.Where(x => x.Id != null).ToList();
        foreach (var item in prodajaAltModels)
            if (item.Id == null)
            {
                var zaknProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == item.MainCatId);
                var part = await _context.MainCats.FindAsync(item.MainCatId);
                if (zaknProd != null)
                {
                    zaknProd.SellCount += item.Count ?? 0;
                }
                else
                {
                    if (part != null) 
                    {
                        await _context.ZakProdCounts.AddAsync(new ZakProdCount
                        {
                            MainCatId = item.MainCatId ?? 1,
                            BuyCount = 0,
                            SellCount = item.Count ?? 0
                        });
                    }
                }


                var prodaja = new Prodaja
                {
                    MainCatId = item.MainCatId,
                    MainName = item.MainName,
                    UniValue = item.UniValue,
                    Count = item.Count ?? 0,
                    Price = item.Price ?? 0,
                    ProdajaId = prodajaMainId,
                    CurrencyId = item.CurrencyInitialId,
                    InitialPrice = item.InitialPrice,
                    Comment = item.Comment
                };
                await _context.Prodajas.AddAsync(prodaja);
                await _context.SaveChangesAsync();

                var prices = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
                if (prices.Any())
                {
                    var count = item.Count * -1;
                    foreach (var pr in prices)
                    {
                        pr.Count += count ?? 0;
                        count = pr.Count;
                        if (pr.Count < 0)
                            pr.Count = 0;
                        if (count >= 0)
                            break;
                    }
                }

                uniIds.Add(part!.Id);
                part.Count -= item.Count ?? 0;
                await _context.SaveChangesAsync();
                await ReCallcMainCount(part.UniId);
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
                    zakAndProd.SellCount += count ?? 0;

                await _context.SaveChangesAsync();

                if (prodajaAlt != null)
                {
                    prodajaAlt.Comment = item.Comment;
                    prodajaAlt.Count = item.Count ?? 0;
                    prodajaAlt.Price = item.Price ?? 0;
                }

                if (cataPrice.Any())
                {
                    if (count != 0)
                    {
                        var c = count * -1;
                        foreach (var pr in cataPrice)
                        {
                            pr.Count += c ?? 0;
                            c = pr.Count;
                            if (pr.Count < 0)
                                pr.Count = 0;

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
                                Count = (count ?? 0) * -1,
                                Price = (item.Price ?? 0) / currency.ToUsd,
                                CurrencyId = currency.Id ?? 2
                            });
                            mainCat.Count += (count ?? 0) * -1;
                            uniIds.Add(mainCat.Id);
                            await _context.SaveChangesAsync();
                        }
                }

                if (mainCat != null)
                {
                    await ReCallcMainCount(mainCat.UniId);
                }
            }

        var mainGroup = await _context.ProdMainGroups.FindAsync(prodajaAltModels.FirstOrDefault().ProdajaId);
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
                    RowColor = mainCat.RowColor,
                    TextColor = mainCat.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            Name = "  Цена:",
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
        }

        await dbTransaction.CommitAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Update,
            Values = transactionId.ToString(),
            Description = $"Редактирована продажа transaction_id={transactionId}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return catas;
    }
    
    public async Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models,
        ProdajaModel mainModel, AgentTransactionModel initTransaction, AgentTransactionModel agentPayment)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        var time = Convert.ToInt32(DateTime.Now.ToString("HH:mm:ss").Replace(":", ""));
        time = time % 100 == 59 ? time - 1 : time;
        var transaction = await AddNewTransactionNoTracking(initTransaction,time.ToString());
        if (agentPayment.AgentId != 0)
            await AddNewTransactionNoTracking(agentPayment,(time+1).ToString());
        
        mainModel.TransactionId = transaction;
        var catas = new List<CatalogueModel>();
        foreach (var model in models)
        {
            var zakAndProd = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
            if (zakAndProd != null)
            {
                zakAndProd.SellCount += model.Count ?? 0;
            }
            else
            {
                var part = await _context.MainCats.FindAsync(model.MainCatId);
                if (part != null)
                    await _context.ZakProdCounts.AddAsync(new ZakProdCount
                    {
                        MainCatId = model.MainCatId ?? 1,
                        BuyCount = 0,
                        SellCount = model.Count ?? 0
                    });
            }

            await _context.SaveChangesAsync();

            var count = model.Count * -1;
            var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
            var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);

            foreach (var p in prices)
            {
                p.Count += count ?? 0;
                count = p.Count;
                if (p.Count < 0)
                    p.Count = 0;
                if (count >= 0)
                    break;
            }

            await _context.SaveChangesAsync();

            if (mainCat != null)
            {
                mainCat.Count -= model.Count ?? 0;
                catas.Add(new CatalogueModel
                {
                    UniId = mainCat.UniId,
                    MainCatId = mainCat.Id,
                    Name = mainCat.Name,
                    Count = mainCat.Count,
                    UniValue = mainCat.UniValue,
                    ProducerId = mainCat.ProducerId,
                    ProducerName = mainCat.Producer.ProducerName,
                    RowColor = mainCat.RowColor,
                    TextColor = mainCat.TextColor,
                    Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                        new CatalogueModel
                        {
                            Name = "  Цена:",
                            UniId = null,
                            MainCatId = x.MainCatId,
                            PriceId = x.Id,
                            Count = x.Count,
                            Price = x.Price,
                            CurrencyId = x.CurrencyId
                        }))
                });
                await ReCallcMainCount(mainCat.UniId);
            }
        }

        var prod = new ProdMainGroup
        {
            AgentId = mainModel.AgentId,
            CurrencyId = mainModel.CurrencyId,
            Datetime = mainModel.Datetime,
            Comment = mainModel.Comment,
            TransactionId = mainModel.TransactionId,
            TotalSum = mainModel.TotalSum,
            Prodajas = models.Select(x => new Prodaja
            {
                Count = x.Count ?? 0,
                MainCatId = x.MainCatId,
                MainName = x.MainName,
                Price = x.Price ?? 0,
                UniValue = x.UniValue,
                InitialPrice = x.InitialPrice,
                CurrencyId = x.CurrencyInitialId,
                Comment = x.Comment
            }).ToList()
        };
        await _context.ProdMainGroups.AddAsync(prod);

        await _context.SaveChangesAsync();

        await dbTransaction.CommitAsync();
        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Write,
            Values = prod.Id.ToString(),
            Description = $"Добавлено продажа id={prod.Id}, agent_id={prod.AgentId}, total_sum={prod.TotalSum}",
            Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });
        return catas;
    }

    public async Task<IEnumerable<CatalogueModel>> DeleteProdajaCountReCalc(int transactionId,
        IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        var catas = new List<CatalogueModel>();

        foreach (var model in prodajaAltModels)
            if (model.MainCatId != null)
            {
                var zakNprod = await _context.ZakProdCounts.FirstOrDefaultAsync(x => x.MainCatId == model.MainCatId);
                if (zakNprod != null)
                    zakNprod.SellCount -= model.Count ?? 0;
                await _context.SaveChangesAsync();

                var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
                var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices)
                    .ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);
                if (prices.Any())
                {
                    prices.First().Count += model.Count ?? 0;
                }
                else
                {
                    var currency = await _context.Currencies.FindAsync(currencyId);

                    if (currency != null)
                        await _context.MainCatPrices.AddAsync(new MainCatPrice
                        {
                            MainCatId = model.MainCatId ?? 5923,
                            Count = model.Count ?? 0,
                            CurrencyId = model.CurrencyInitialId,
                            Price = model.InitialPrice
                        });
                }

                await _context.SaveChangesAsync();

                if (mainCat != null)
                {
                    mainCat.Count += model.Count ?? 0;
                    await _context.SaveChangesAsync();
                    catas.Add(new CatalogueModel
                    {
                        UniId = mainCat.UniId,
                        MainCatId = mainCat.Id,
                        Name = mainCat.Name,
                        Count = mainCat.Count,
                        UniValue = mainCat.UniValue,
                        ProducerId = mainCat.ProducerId,
                        ProducerName = mainCat.Producer.ProducerName,
                        RowColor = mainCat.RowColor,
                        TextColor = mainCat.TextColor,
                        Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                            new CatalogueModel
                            {
                                Name = "  Цена:",
                                UniId = null,
                                MainCatId = x.MainCatId,
                                PriceId = x.Id,
                                Count = x.Count,
                                Price = x.Price,
                                CurrencyId = x.CurrencyId
                            }))
                    });
                }

                await ReCallcMainCount(model.MainCatId ?? 5923);
            }

        var transaction = await ReCalcTransactions(transactionId);
        if (transaction != null)
            _context.AgentTransactions.Remove(transaction);

        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        await AddRecordToLog(new Action
        {
            Action1 = (int)ActionsEnum.Delete,
            Values = transactionId.ToString(),
            Description = $"Удалено продажи transaction_id={transactionId}", Seen = 0,
            Date = Converters.ToDateTimeSqlite(DateTime.Now.Date.ToString("dd.MM.yyyy")),
            Time = DateTime.Now.ToShortTimeString()
        });

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
            var balance = await _context.AgentBalances.FirstOrDefaultAsync(x =>
                x.AgentId == transaction.AgentId && x.CurrencyId == transaction.Currency);
            balance!.Balance -= transaction.TransactionSum;
            var transactionSum = transaction.TransactionSum;
            FormattableString query =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} <= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency}";

            var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
            afterTransactions.Remove(afterTransactions.Where(x =>
                x.TransactionDatatime == transaction.TransactionDatatime &&
                Convert.ToInt32(x.Time) <= Convert.ToInt32(transaction.Time)).ToList());
            foreach (var tr in afterTransactions) tr.Balance -= transactionSum;
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
            var balance = await _context.AgentBalances.FirstOrDefaultAsync(x =>
                x.AgentId == transaction.AgentId && x.CurrencyId == transaction.Currency);
            balance!.Balance += diff;
            FormattableString sameDate =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} = transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency} and time > {transaction.Time}";
            FormattableString otherDate =
                $"SELECT * from agent_transactions where {transaction.TransactionDatatime} < transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency}";

            var afterTransactions = await _context.AgentTransactions.FromSql(sameDate).ToListAsync();
            afterTransactions.AddRange(await _context.AgentTransactions.FromSql(otherDate).ToListAsync());
            
            foreach (var tr in afterTransactions)
                tr.Balance += diff;
            await _context.SaveChangesAsync();
            return transaction;
        }

        return transaction;
    }

    public async Task SetMainCatImg(int? mainCatId, byte[]? img)
    {
        if (mainCatId != null)
        {
            var part = await _context.MainCats.FindAsync(mainCatId);
            if (part != null)
            {
                if (part.ImageId != null)
                {
                    var imge = await _context.Images.FindAsync(part.ImageId);
                    if (imge != null)
                        _context.Images.Remove(imge);
                }

                var imgModel = new Image { Img = img };
                await _context.Images.AddAsync(imgModel);
                await _context.SaveChangesAsync();

                part.ImageId = imgModel.Id;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task EditProducerById(int producerId, string newName)
    {
        var producer = await _context.Producers.FindAsync(producerId);
        if (producer != null)
        {
            producer.ProducerName = newName;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ProducerModel?> AddNewProducer(string producerName)
    {
        var producer = new Producer { ProducerName = producerName };
        await _context.Producers.AddAsync(producer);
        await _context.SaveChangesAsync();

        return new ProducerModel { Id = producer.Id, ProducerName = producerName };
    }

    public async Task<bool> DeleteProducer(int producerId)
    {
        var producer = await _context.Producers.FindAsync(producerId);
        if (producer != null)
        {
            var parts = await _context.MainCats.Where(x => x.ProducerId == producerId).ToListAsync();
            if (parts.Any())
            {
                foreach (var item in parts)
                    item.ProducerId = 1;
            }

            _context.Producers.Remove(producer);
            await _context.SaveChangesAsync();
            return true;
        }
        else
            return false;
    }

    public async Task<CatalogueModel?> EditColor(string rowColor, string textColor, int id)
    {
        var mainCat = await _context.MainCats.Include(x => x.MainCatPrices).ThenInclude(x => x.Currency)
            .Include(x => x.Producer).FirstOrDefaultAsync(x => x.Id == id);

        if (mainCat != null)
        {
            mainCat.RowColor = rowColor;
            mainCat.TextColor = textColor;
            await _context.SaveChangesAsync();

            return new CatalogueModel
            {
                UniId = mainCat.UniId,
                MainCatId = mainCat.Id,
                Name = mainCat.Name,
                Count = mainCat.Count,
                UniValue = mainCat.UniValue,
                ProducerId = mainCat.ProducerId,
                ProducerName = mainCat.Producer.ProducerName,
                RowColor = mainCat.RowColor,
                TextColor = mainCat.TextColor,
                Children = new ObservableCollection<CatalogueModel>(mainCat.MainCatPrices.Select(x =>
                    new CatalogueModel
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
            };
        }

        return null;
    }

    private async Task AddRecordToLog(Action action)
    {
        var act = await _context.Actions.AddAsync(action);
        await _context.SaveChangesAsync();
    }
}