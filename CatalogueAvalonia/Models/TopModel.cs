using CatalogueAvalonia.Model;
using CatalogueAvalonia.Services.DataBaseAction;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Models
{
	public class TopModel
	{
		private readonly IDataBaseProvider _dataBaseProvider;
		private readonly IDataBaseAction _dataBaseAction;
		public TopModel(IDataBaseProvider dataBaseProvider, IDataBaseAction dataBaseAction)
		{
			_dataBaseProvider = dataBaseProvider;
			_dataBaseAction = dataBaseAction;
		}
		/// <summary>
		/// Получает все запчасти из каталога.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<CatalogueModel>> GetCatalogueAsync()
		{
			return await _dataBaseProvider.GetCatalogueAsync();
		}
		/// <summary>
		/// Удаляет основную группу запчастей по id основной группы.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task DeleteGroupFromCatalogue(int? id)
		{
			await _dataBaseAction.DeleteMainNameById(id);
		}
		/// <summary>
		/// Удаляет запчасть по id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task DeleteSoloFromCatalogue(int? id)
		{
			await _dataBaseAction.DeleteFromMainCatById(id);
		}
		/// <summary>
		/// Получает всех производителей из базы данных.
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<ProducerModel>> GetProducersAsync()
		{
			return await _dataBaseProvider.GetProducersAsync();
		}
		/// <summary>
		/// Редактирует группу запчастей.
		/// </summary>
		/// <param name="catalogue"></param>
		/// <returns></returns>
		public async Task EditCatalogueAsync(CatalogueModel catalogue)
		{
			await _dataBaseAction.EditCatalogue(catalogue).ConfigureAwait(false);
		}
		/// <summary>
		/// Получает группу запчастей по основному id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<CatalogueModel> GetCatalogueByIdAsync(int id)
		{
			return await _dataBaseProvider.GetCatalogueById(id).ConfigureAwait(false);
		}
	}
}
