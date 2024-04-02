using CatalogueAvalonia.Core;
using CatalogueAvalonia.Model;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
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

			Task.Run(LoadLazy);
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
			var where = message.Value.Where;
			if (where == "PartCatalogue")
			{
				var what = _catalogueModels.Find(x => x.UniId == message.Value.Id);
				if (what != null)
					_catalogueModels.Remove(what);
			}
			else if (where == "Agent")
			{
				var what = _agentModels.Find(x => x.Id == message.Value.Id);
				if (what != null)
					_agentModels.Remove(what);
			}
			else if (where == "CataloguePrices")
			{
				int? uniId = message.Value.SecondId;
				int? mainCatId = message.Value.Id;
				if (uniId != null && mainCatId != null)
				{
					var mainName = _catalogueModels.Where(x => x.UniId == uniId).Single();
					if (mainName.Children != null)
					{
						var mainCats = mainName.Children.Where(x => x.MainCatId == mainCatId).Single();
						if (mainCats.Children != null)
						{
							mainCats.Count = 0;
							mainCats.Children.Clear();
						}
					}
				}
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
					var item =_catalogueModels.Where(x => x.UniId == uniId).SingleOrDefault();
					if (item != null)
					{
						_catalogueModels.ReplaceOrAdd(item, model);
					}
				}
			}
			else if(where == "Currencies")
			{
				var what = message.Value.What as IEnumerable<CurrencyModel>;
				if (what != null)
				{
					_currencyModels.Clear();
					_currencyModels.AddRange(what);
				}
			}
			else if (where == "CataloguePrices")
			{
				var what = (CatalogueModel?)message.Value.What;
				if (what != null)
				{
					var mainName = _catalogueModels.Single(x => x.UniId == what.UniId);
					if (mainName.Children != null)
					{
						var mainCats = mainName.Children.Single(x => x.MainCatId == what.MainCatId);
						if (mainCats != null)
						{
							mainName.Children.ReplaceOrAdd(mainCats, what);
						}
					}
				}
			}
			else if (where == "CataloguePricesList")
			{
				var what = (IEnumerable<CatalogueModel>?)message.Value.What;
				if (what != null)
				{
					foreach (var item in what)
					{
						var mainName = _catalogueModels.Single(x => x.UniId == item.UniId);
						if (mainName.Children != null)
						{
							var mainCats = mainName.Children.Single(x => x.MainCatId == item.MainCatId);
							if (mainCats != null)
							{
								mainName.Children.ReplaceOrAdd(mainCats, item);
							}
						}
					}
				}
			}
		}

		//to initialize Store
		public async Task LoadLazy()
		{
			await _lazyInit.Value;
			Messenger.Send(new ActionMessage("DataBaseLoaded"));
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
		}
	}
}
