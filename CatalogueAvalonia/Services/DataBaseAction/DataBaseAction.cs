using Avalonia.Remote.Protocol;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using DataBase.Data;
using FluentAvalonia.UI.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public class DataBaseAction : IDataBaseAction
	{
		private readonly DataContext _context;
		public DataBaseAction(DataContext dataContext) { _context = dataContext; }
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
			MainName? mainName = await _context.MainNames.FindAsync(uniId);
			foreach (var id in deleteIds) 
			{
				var mainCat = await _context.MainCats.FindAsync(id);
				if (mainCat != null) 
				{
					_context.Remove(mainCat);
				}
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
							UniId = uniId ?? 5923,
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
			else
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

			return new AgentModel { Id = model.Id, IsZak = model.IsZak, Name = model.Name};
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
				Balance = Math.Round(agentTransaction.Balance, 2)
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
				double transactionSum = transaction.TransactionSum;
				FormattableString query = $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {agentId} and currency = {currencyId} and id > {transactionId}";
				_context.AgentTransactions.Remove(transaction);

				var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
				foreach (var tr in afterTransactions) 
				{
					tr.Balance = Math.Round(tr.Balance - transactionSum, 2);
				}
				await _context.SaveChangesAsync();
			}
		}
		public async Task<CatalogueModel?> EditMainCatPrices(IEnumerable<MainCatPriceModel> mainCatPrices, int mainCatId)
		{
			var mainCat = await _context.MainCats.FindAsync(mainCatId);
			if (mainCat != null)
			{
				if (mainCatPrices.Any())
				{
					mainCat.MainCatPrices = mainCatPrices.Select(x => new MainCatPrice
					{
						Count = x.Count,
						CurrencyId = x.CurrencyId,
						Price = x.Price,
					}).ToList();
					mainCat.Count = mainCatPrices.Sum(x => x.Count);
					await _context.SaveChangesAsync().ConfigureAwait(true);

					if (mainCat.MainCatPrices.Where(x => x.Currency == null).Any())
						return new CatalogueModel
						{
							UniId = mainCat.UniId,
							MainCatId = mainCat.Id,
							Name = mainCat.Name,
							Count = mainCat.Count,
							UniValue = mainCat.UniValue,
							ProducerId = mainCat.ProducerId,
							ProducerName = mainCat.Producer.ProducerName,
							Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
							{
								UniId = null,
								MainCatId = x.MainCatId,
								PriceId = x.Id,
								Count = x.Count,
								Price = x.Price,
								CurrencyId = x.CurrencyId
							}))
						};
					else
						return new CatalogueModel
						{
							UniId = mainCat.UniId,
							MainCatId = mainCat.Id,
							Name = mainCat.Name,
							Count = mainCat.Count,
							UniValue = mainCat.UniValue,
							ProducerId = mainCat.ProducerId,
							ProducerName = mainCat.Producer.ProducerName,
							Children = new (mainCat.MainCatPrices.Select(x => new CatalogueModel
							{
								UniId = null,
								MainCatId = x.MainCatId,
								PriceId = x.Id,
								Count = x.Count,
								Price = x.Price,
								CurrencyId = x.CurrencyId,
								CurrencyName = x.Currency.CurrencyName,
							}))

						};
				}
				else
				{
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
			}
			else
				return null;

		}

		public async Task DeleteMainCatPricesById(int mainCatId)
		{
			var prices = await _context.MainCatPrices.Where(x => x.MainCatId == mainCatId).ToListAsync();

			if (prices != null)
			{
				_context.MainCatPrices.RemoveRange(prices);
			}
			await _context.SaveChangesAsync();
		}
		public async Task EditCurrency(IEnumerable<CurrencyModel> currencyModels, IEnumerable<int> deletedIds)
		{
			foreach (var item in deletedIds)
			{
				if (item != 1)
				{
					var entity = await _context.Currencies.FindAsync(item);
					if (entity != null)
					{
						_context.Remove(entity);
					}
				}
			}
			await _context.SaveChangesAsync();



			await _context.AddRangeAsync(currencyModels.Where(x => x.Id == null).Select(x =>new Currency
			{
				CanDelete = x.CanDelete,
				CurrencyName = x.CurrencyName,
				CurrencySign = x.CurrencySign,
				ToUsd = x.ToUsd
			}));
			foreach (var item in currencyModels.Where(x => x.Id != null)) 
			{
				var curr = await _context.Currencies.FindAsync(item.Id);
				if (curr != null)
				{
					curr.CurrencyName = item.CurrencyName;
					curr.CurrencySign = item.CurrencySign;
					curr.ToUsd = item.ToUsd;
				}

			}

			await _context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task DeleteAllCurrencies()
		{
			_context.Currencies.RemoveRange(await _context.Currencies.Where(x => x.Id != 0 || x.Id != 2).ToListAsync());
			await _context.SaveChangesAsync();
		}
		public async Task AddNewZakupka(IEnumerable<ZakupkaAltModel> zakupka, ZakupkiModel zakMain)
		{
			await _context.ZakMainGroups.AddAsync(new ZakMainGroup
			{
				TransactionId = zakMain.TransactionId,
				Datetime = Converters.ToDateTimeSqlite(zakMain.Datetime),
				AgentId = zakMain.AgentId,
				CurrencyId = zakMain.CurrencyId,
				TotalSum = zakMain.TotalSum,
				Zakupkas = zakupka.Select(x => new Zakupka
				{
					Count = x.Count,
					MainCatId = x.MainCatId,
					Price = x.Price,
					MainName = x.MainName,
					UniValue = x.UniValue
				}).ToList(),
				
			});
			await _context.SaveChangesAsync();
		}
		public async Task<IEnumerable<CatalogueModel>> AddNewPricesForParts(IEnumerable<ZakupkaAltModel> parts, int currencyId)
		{
			List<CatalogueModel> cata = new List<CatalogueModel>();
			await _context.MainCatPrices.AddRangeAsync(parts.Select(x => new MainCatPrice
			{
				Count = x.Count,
				CurrencyId = currencyId,
				MainCatId = x.MainCatId ?? default,
				Price = x.Price
			}));
			await _context.SaveChangesAsync();	

			foreach(var part in parts)
			{
				var item = await _context.MainCats.FindAsync(part.MainCatId);
				if (item != null)
				{
					item.Count += part.Count;
					await _context.SaveChangesAsync();
				}

				var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices).ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == part.MainCatId);
				if (mainCat != null)
				{
					cata.Add(new CatalogueModel
					{
						UniId = mainCat.UniId,
						MainCatId = mainCat.Id,
						Name = mainCat.Name,
						Count = mainCat.Count,
						UniValue = mainCat.UniValue,
						ProducerId = mainCat.ProducerId,
						ProducerName = mainCat.Producer.ProducerName,
						Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
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
		public async Task<IEnumerable<CatalogueModel>> DeleteZakupkaWithCountReCalc(int transactionId, IEnumerable<ZakupkaAltModel> zakupkaAltModels)
		{
			List<CatalogueModel> uniIds = new List<CatalogueModel>();
			foreach (var item in zakupkaAltModels)
			{
				if (item.MainCatId != null)
				{
					int count = item.Count;
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
						{
							if (pr.Count <= 0)
							{
								var cataPrice = await _context.MainCatPrices.FindAsync(pr.Id);
								if (cataPrice != null)
									_context.MainCatPrices.Remove(cataPrice);
							}
						}
						cataPrices.RemoveAll(x => x.Count <= 0);
					}

					await _context.SaveChangesAsync();

					var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices).ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item.MainCatId);
					if (mainCat != null)
					{
						mainCat.Count = await _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId).SumAsync(x => x.Count);
						uniIds.Add(new CatalogueModel
						{
							UniId = mainCat.UniId,
							MainCatId = mainCat.Id,
							Name = mainCat.Name,
							Count = mainCat.Count,
							UniValue = mainCat.UniValue,
							ProducerId = mainCat.ProducerId,
							ProducerName = mainCat.Producer.ProducerName,
							Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
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
		public async Task<IEnumerable<CatalogueModel>> EditZakupka(IEnumerable<int> deletedIds, IEnumerable<ZakupkaAltModel> zakupkaAlts, Dictionary<int, int> lastCounts, CurrencyModel currency, string date, double totalSum, int transactionId)
		{
			List<int> uniIds = new List<int>();
			
			foreach (var item in deletedIds)
			{
				var zakupkaAltItem = await _context.Zakupkas.FindAsync(item);
				if (zakupkaAltItem != null)
				{
					var prices = _context.MainCatPrices.Where(x => x.MainCatId == zakupkaAltItem.MainCatId);
					var cataloguePart = await _context.MainCats.FindAsync(zakupkaAltItem.MainCatId);

					int count = zakupkaAltItem.Count;
					int pricesTotalCount = prices.Sum(x => x.Count);

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
						foreach(var price in prices)
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
			var zakCounts = zakupkaAlts.Where(x => x.Id != null).ToList();
			foreach(var item in zakupkaAlts)
			{
				if (item.Id == null)
				{
					_context.Zakupkas.Add(new Zakupka 
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
						Price = Math.Round(item.Price / currency.ToUsd, 2),

					});
					var cata = await _context.MainCats.FindAsync(item.MainCatId);
					if (cata != null)
					{
						cata.Count = item.Count;
						uniIds.Add(cata.Id);
					}
				}
				else
				{
					var cataPrice = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId);
					var zakupkaAlt = await _context.Zakupkas.FindAsync(item.Id);
					var mainCat = await _context.MainCats.FindAsync(item.MainCatId);

					int totalCount = cataPrice.Sum(x => x.Count);
					int prevCount = lastCounts[item.MainCatId ?? 0] / zakCounts.Where(x => x.MainCatId == item.MainCatId).Count();
					lastCounts[item.MainCatId ?? 0] -= prevCount;
					int currCount = item.Count;
					int count = currCount - prevCount;


					if (zakupkaAlt != null)
					{
						zakupkaAlt.Count = item.Count;
						zakupkaAlt.Price = item.Price;
					}
					if(totalCount <= 0)
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
								CurrencyId = currency.Id ?? 0,
								MainCatId = item.MainCatId ?? 0,
								Price = Math.Round(item.Price / currency.ToUsd, 2)
							});
							if (mainCat != null)
							{
								mainCat.Count = count;
								uniIds.Add(mainCat.Id);
							}
						}
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
									if (count >= 0)
										break;
								}
								_context.MainCatPrices.RemoveRange(cataPrice.Where(x => x.Count <= 0));
								if (mainCat != null)
								{
									mainCat.Count = _context.MainCatPrices.Where(x => x.MainCatId == item.MainCatId && x.Count > 0).Sum(x => x.Count);
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
						}
					}
					var a = zakCounts.FirstOrDefault(x => x.MainCatId == item.MainCatId);
					if (a != null)
						zakCounts.Remove(a);
				}
				
			}
			var mainGroup = await _context.ZakMainGroups.FindAsync(zakupkaAlts.First().ZakupkaId);
			if (mainGroup != null)
			{
				mainGroup.TotalSum = totalSum;
				mainGroup.Datetime = date;
				mainGroup.CurrencyId = currency.Id ?? 2;
			}

			var tr = await _context.AgentTransactions.FindAsync(transactionId);
			if (tr != null)
			{
				double diff = -1 * (tr.TransactionSum + totalSum);
				var transaction = await ReCalcTransactions(transactionId, diff);
			}
			

			await _context.SaveChangesAsync();

			List<CatalogueModel> catas = new List<CatalogueModel>();
			foreach(int item in uniIds.Distinct().ToList())
			{
				var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices).ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == item);
                if (mainCat != null)
                {
					catas.Add(new CatalogueModel
					{
						UniId = mainCat.UniId,
						MainCatId = mainCat.Id,
						Name = mainCat.Name,
						Count = mainCat.Count,
						UniValue = mainCat.UniValue,
						ProducerId = mainCat.ProducerId,
						ProducerName = mainCat.Producer.ProducerName,
						Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
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
			return catas;
		}
		public async Task<IEnumerable<CatalogueModel>> AddNewProdaja(IEnumerable<ProdajaAltModel> models, ProdajaModel mainModel)
		{
			List<CatalogueModel> catas = new List<CatalogueModel>();
			foreach (var model in models) 
			{
				int count = model.Count * -1;
				var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
				var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices).ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);

				foreach (var p in prices) 
				{
					p.Count += count;
					count = p.Count;
					if (count >= 0)
						break;
				}
				foreach (var item in prices)
				{
					if (item.Count <= 0)
					{
						_context.MainCatPrices.Remove(item);
					}
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
						Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
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
				}).ToList()
			});

			await _context.SaveChangesAsync();
			return catas;
		}
		public async Task<IEnumerable<CatalogueModel>> DeleteProdajaCountReCalc(int transactionId, IEnumerable<ProdajaAltModel> prodajaAltModels, int currencyId)
		{
			List<CatalogueModel> catas = new List<CatalogueModel>();

			foreach (var model in prodajaAltModels)
			{
				if (model.MainCatId != null)
				{
					var prices = _context.MainCatPrices.Where(x => x.MainCatId == model.MainCatId);
					var mainCat = await _context.MainCats.Include(x => x.Producer).Include(x => x.MainCatPrices).ThenInclude(x => x.Currency).FirstOrDefaultAsync(x => x.Id == model.MainCatId);
					if (prices.Any())
						prices.First().Count += model.Count;
					else
					{
						var currency = await _context.Currencies.FindAsync(currencyId);

						if (currency != null)
							await _context.MainCatPrices.AddAsync(new MainCatPrice
							{
								MainCatId = model.MainCatId ?? 5923,
								Count = model.Count,
								CurrencyId = currency.Id,
								Price = Math.Round(model.Price/currency.ToUsd, 2)
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
							Children = new(mainCat.MainCatPrices.Select(x => new CatalogueModel
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
			var transaction = await ReCalcTransactions(transactionId);
			if (transaction != null)
				_context.AgentTransactions.Remove(transaction);

			await _context.SaveChangesAsync();
			return catas;
		}
		private async Task<AgentTransaction?> ReCalcTransactions(int transactionId)
		{
			var transaction = await _context.AgentTransactions.FindAsync(transactionId);
			if (transaction != null)
			{
				double transactionSum = transaction.TransactionSum;
				FormattableString query = $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency} and id > {transactionId}";

				var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
				foreach (var tr in afterTransactions)
				{
					tr.Balance = Math.Round(tr.Balance - transactionSum, 2);
				}
				await _context.SaveChangesAsync();
				return transaction;
			}
			else
				return transaction;
		}
		private async Task<AgentTransaction?> ReCalcTransactions(int transactionId, double diff)
		{
			var transaction = await _context.AgentTransactions.FindAsync(transactionId);
			if (transaction != null)
			{
				FormattableString query = $"SELECT * from agent_transactions where {transaction.TransactionDatatime} >= transaction_datatime and agent_id = {transaction.AgentId} and currency = {transaction.Currency} and id > {transactionId}";

				var afterTransactions = await _context.AgentTransactions.FromSql(query).ToListAsync();
				foreach (var tr in afterTransactions)
					tr.Balance = Math.Round(tr.Balance + diff, 2);
				await _context.SaveChangesAsync();
				return transaction;
			}
			else
				return transaction;
		}
	}
	
}
