using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Services.DataStore
{
	public class DataStore : ObservableRecipient
	{
		private readonly TopModel _topModel;
		private readonly Lazy<Task> _lazyInit;

		private readonly List<CatalogueModel> _catalogueModels;
		public List<CatalogueModel> CatalogueModels => _catalogueModels;
		public DataStore(IMessenger messenger) : base(messenger)
		{
			_topModel = new TopModel();
			_lazyInit = new Lazy<Task>(LoadAll);
			_catalogueModels = new List<CatalogueModel>();
		}
		//to initialize Store
		public async Task LoadLazy()
		{
			await _lazyInit.Value.ConfigureAwait(false);
		}

		public async Task LoadAll()
		{
			IEnumerable<CatalogueModel> catalogueModels = await _topModel.GetCatalogueAsync().ConfigureAwait(false);
			_catalogueModels.Clear();
			_catalogueModels.AddRange(catalogueModels);


			Messenger.Send(new DataBaseLoadedMessage(""));
		}
	}
}
