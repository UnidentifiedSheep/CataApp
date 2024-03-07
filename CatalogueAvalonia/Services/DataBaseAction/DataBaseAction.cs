using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using DataBase.Data;
using FluentAvalonia.UI.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
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

		public async Task EditCatalogue(CatalogueModel catalogue)
		{
			var uniId = catalogue.UniId;
			MainName? mainName = await _context.MainNames.FindAsync(uniId);

			if (mainName != null && catalogue.Children != null)
			{
				mainName.Name = catalogue.Name;
				mainName.MainCats = catalogue.Children.Select(x => new MainCat
				{
					Id = x.MainCatId ?? default,
					Name = x.Name,
					ProducerId = x.ProducerId,
					UniValue = x.UniValue
				}).ToList();
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
				TransactionSum = Math.Round(agentTransaction.TransactionSum, 2),
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
					tr.Balance -= transactionSum;
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
	}
}
