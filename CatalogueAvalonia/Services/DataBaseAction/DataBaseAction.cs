using CatalogueAvalonia.Models;
using DataBase.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			if(el != null)
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
	}
}
