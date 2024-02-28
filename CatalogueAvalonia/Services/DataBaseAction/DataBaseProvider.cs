using Avalonia;
using CatalogueAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataBase.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CatalogueAvalonia.Services.DataBaseAction
{
	public class DataBaseProvider : IDataBaseProvider
	{
		public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
		{
			using(var context = new DataContext())
			{
				var models = await context.MainNames.Include(x => x.MainCats)
													.ThenInclude(x => x.MainCatPrices)
													.ThenInclude(x => x.Currency)
													.Include(x => x.MainCats)
													.ThenInclude(x => x.Producer)
													.ToListAsync().ConfigureAwait(false);
				return models.Select(x => new CatalogueModel
				{
					UniId = x.UniId,
					Name = x.Name,
					Children = new(x.MainCats.Select(x => new CatalogueModel
					{
						UniValue = x.UniValue,
						Name = x.Name,
						Count = x.Count,
						ProducerId = x.ProducerId,
						ProducerName = x.Producer.ProducerName,
						MainCatId = x.Id,
						Children = new(x.MainCatPrices.Select(x => new CatalogueModel
						{
							Count = x.Count,
							Price = x.Price,
							PriceId = x.Id,
							CurrencyId = x.CurrencyId,
							CurrencyName = x.Currency.CurrencyName
						}))
					}))
				});
			}
		}
	}
}
