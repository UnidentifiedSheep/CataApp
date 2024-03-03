using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
		private readonly List<AgentModel> _agentModels;
		public List<AgentModel> AgentModels => _agentModels;
		private readonly List<CurrencyModel> _currencyModels;
		public List<CurrencyModel> CurrencyModels => _currencyModels;
		public DataStore(TopModel topModel, IMessenger messenger) : base(messenger)
		{
			_topModel = topModel;
			_lazyInit = new Lazy<Task>(LoadAll);
			_catalogueModels = new List<CatalogueModel>();
			_producerModels = new List<ProducerModel>();
			_agentModels = new List<AgentModel>();
			_currencyModels = new List<CurrencyModel>();

			Messenger.Register<EditedMessage>(this, OnDataBaseEdited);
			Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
			Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
		}

		private void OnDataBaseAdded(object recipient, AddedMessage message)
		{
			if (message.Value.Where == "Catalogue")
			{
				var what = message.Value.What as CatalogueModel;
				if (what != null)
					_catalogueModels.Add(what);
			}
			else if (message.Value.Where == "Agent")
			{
				var what = (AgentModel?)message.Value.What;
				if (what != null)
					_agentModels.Add(what);
			}
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
			string where = message.Value.Where;
			if (where == "PartCatalogue") 
			{
				var uniId = message.Value.Id;
				var model = (CatalogueModel?)message.Value.What;
				if (model != null)
				{
					_catalogueModels.RemoveAll(x => x.UniId == uniId);
					_catalogueModels.Add(model);
				}
			}
			else if (where == "Agents")
			{
				var id = message.Value.Id;
				_agentModels.RemoveAll(x => x.Id == id);
				var model = (AgentModel?)message.Value.What;
				if (model != null)
					_agentModels.Add(model);
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

			_agentModels.Clear();
			_agentModels.AddRange(await _topModel.GetAllAgentsAsync().ConfigureAwait(false));

			_currencyModels.Clear();
			_currencyModels.AddRange(await _topModel.GetAllCurrenciesAsync().ConfigureAwait(false));

			Messenger.Send(new DataBaseLoadedMessage(""));
		}
	}
}
