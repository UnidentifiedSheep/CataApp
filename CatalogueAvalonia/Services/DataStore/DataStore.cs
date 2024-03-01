using CatalogueAvalonia.Model;
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

		private readonly List<ProducerModel> _producerModels;
		public List<ProducerModel> ProducerModels => _producerModels;
		public DataStore(TopModel topModel, IMessenger messenger) : base(messenger)
		{
			_topModel = topModel;
			_lazyInit = new Lazy<Task>(LoadAll);
			_catalogueModels = new List<CatalogueModel>();
			_producerModels = new List<ProducerModel>();

			Messenger.Register<EditedMessage>(this, OnDataBaseEdited);
			Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
		}

		private void OnDataBaseDeleted(object recipient, DeletedMessage message)
		{
			if (message.Value.Where == "PartCatalogue") 
			{
				_catalogueModels.RemoveAll(x => x.UniId == message.Value.Id);
			}
		}

		private void OnDataBaseEdited(object recipient, EditedMessage message)
		{
			if (message.Value.Where == "PartCatalogue") 
			{
				var uniId = message.Value.Id;
				var model = (CatalogueModel?)message.Value.What;
				if (model != null)
				{
					_catalogueModels.RemoveAll(x => x.UniId == uniId);
					_catalogueModels.Add(model);

				}
			}
		}

		//to initialize Store
		public async Task LoadLazy()
		{
			await _lazyInit.Value.ConfigureAwait(false);
		}

		public async Task LoadAll()
		{
			_catalogueModels.Clear();
			_catalogueModels.AddRange(await _topModel.GetCatalogueAsync().ConfigureAwait(false));

			_producerModels.Clear();
			_producerModels.AddRange(await _topModel.GetProducersAsync().ConfigureAwait(false));


			Messenger.Send(new DataBaseLoadedMessage(""));
		}
	}
}
