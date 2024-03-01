using Avalonia;
using CatalogueAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataBase.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatalogueAvalonia.Model;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public class DataBaseProvider : IDataBaseProvider
	{
		private readonly DataContext _context;
		public DataBaseProvider(DataContext dataContext) 
		{ 
			_context = dataContext; 
		}
		public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
		{
			var models = await _context.MainNames.Include(x => x.MainCats)
												 .ThenInclude(x => x.MainCatPrices)
											   	 .ThenInclude(x => x.Currency)
												 .Include(x => x.MainCats)
												 .ThenInclude(x => x.Producer)
												 .ToListAsync().ConfigureAwait(false);
			return models.Select(x => new CatalogueModel
			{
				UniId = x.UniId,
				Name = x.Name,
				MainCatId = null,
				Children = new(x.MainCats.Select(x => new CatalogueModel
				{
					UniId = x.UniId,
					UniValue = x.UniValue,
					Name = x.Name,
					Count = x.Count,
					ProducerId = x.ProducerId,
					ProducerName = x.Producer.ProducerName,
					MainCatId = x.Id,
					Children = new(x.MainCatPrices.Select(x => new CatalogueModel
					{
						UniId = null,
						MainCatId = null,
						PriceId = x.Id,
						Count = x.Count,
						Price = x.Price,
						CurrencyId = x.CurrencyId,
						CurrencyName = x.Currency.CurrencyName
					}))
				}))
			}) ;
		}

		public async Task<CatalogueModel> GetCatalogueById(int uniId)
		{
			var model = await _context.MainNames.Where(x => x.UniId == uniId).Include(x => x.MainCats)
												 .ThenInclude(x => x.MainCatPrices)
												 .ThenInclude(x => x.Currency)
												 .Include(x => x.MainCats)
												 .ThenInclude(x => x.Producer).FirstAsync();
			if (model != null)
			{
				return new CatalogueModel
				{
					UniId = model.UniId,
					Name = model.Name,
					MainCatId = null,
					Children = new(model.MainCats.Select(x => new CatalogueModel
					{
						UniId = x.UniId,
						UniValue = x.UniValue,
						Name = x.Name,
						Count = x.Count,
						ProducerId = x.ProducerId,
						ProducerName = x.Producer.ProducerName,
						MainCatId = x.Id,
						Children = new(x.MainCatPrices.Select(x => new CatalogueModel
						{
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
			}
			else
				return new CatalogueModel { };
		}

		public async Task<IEnumerable<ProducerModel>> GetProducersAsync()
		{
			return await _context.Producers.Select(x => new ProducerModel() { Id = x.Id, ProducerName = x.ProducerName}).ToListAsync();
		}
	}
}
