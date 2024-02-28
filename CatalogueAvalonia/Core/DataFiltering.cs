using CatalogueAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Core
{
	public class DataFiltering
	{
		private static readonly Regex reg = new Regex(@"[^a-zА-Яа-яA-Z0-9_]+");

		public static async IAsyncEnumerable<CatalogueModel> FilterByMainName(List<CatalogueModel> catalogueModels, string objectToFind, [EnumeratorCancellation] CancellationToken token)
		{
			for (int i = 0; i < catalogueModels.Count(); i++) 
			{ 
				if(!string.IsNullOrEmpty(objectToFind))
				{
					if (await CheckIfContainsName(catalogueModels[i], objectToFind).ConfigureAwait(false))
					{
						yield return catalogueModels[i];
					}
				}
			}
		}
		private static Task<bool> CheckIfContainsName(CatalogueModel catalogueModel, string ObjectToFind)
		{
			return Task.FromResult(reg.Replace(catalogueModel.Name, "").ToLower().Contains(reg.Replace(ObjectToFind, "").ToLower()));
		}
	}
}
