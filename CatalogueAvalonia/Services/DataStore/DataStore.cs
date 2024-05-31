using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;

namespace CatalogueAvalonia.Services.DataStore;

public class DataStore : ObservableRecipient
{
    private readonly Lazy<Task> _lazyInit;

    private readonly TopModel _topModel;

    public DataStore(TopModel topModel, IMessenger messenger) : base(messenger)
    {
        _topModel = topModel;
        _lazyInit = new Lazy<Task>(LoadAll);
        CatalogueModels = new List<CatalogueModel>();
        ProducerModels = new List<ProducerModel>();
        AgentModels = new List<AgentModel>();
        CurrencyModels = new List<CurrencyModel>();

        Messenger.Register<EditedMessage>(this, OnDataBaseEdited);
        Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
        Messenger.Register<AddedMessage>(this, OnDataBaseAdded);

        Task.Run(LoadLazy);
    }

    public List<CatalogueModel> CatalogueModels { get; }

    public List<ProducerModel> ProducerModels { get; }

    public List<AgentModel> AgentModels { get; }

    public List<CurrencyModel> CurrencyModels { get; }

    private void OnDataBaseAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Catalogue")
        {
            var what = message.Value.What as CatalogueModel;
            if (what != null)
                CatalogueModels.Add(what);
        }
        else if (message.Value.Where == "Agent")
        {
            var what = (AgentModel?)message.Value.What;
            if (what != null)
                AgentModels.Add(what);
        }
        else if(message.Value.Where == "Producer")
        {
            var what = (ProducerModel?)message.Value.What;
            if (what != null)
                ProducerModels.Add(what);
        }
    }

    private void OnDataBaseDeleted(object recipient, DeletedMessage message)
    {
        var where = message.Value.Where;
        if (where == "PartCatalogue")
        {
            var what = CatalogueModels.Find(x => x.UniId == message.Value.Id);
            if (what != null)
                CatalogueModels.Remove(what);
        }
        else if (where == "Agent")
        {
            var what = AgentModels.Find(x => x.Id == message.Value.Id);
            if (what != null)
                AgentModels.Remove(what);
        }
        else if (where == "CataloguePrices")
        {
            var uniId = message.Value.SecondId;
            var mainCatId = message.Value.Id;
            if (uniId != null && mainCatId != null)
            {
                var mainName = CatalogueModels.Single(x => x.UniId == uniId);
                if (mainName.Children != null)
                {
                    var mainCats = mainName.Children.Single(x => x.MainCatId == mainCatId);
                    if (mainCats.Children != null)
                    {
                        mainCats.Count = 0;
                        mainCats.Children.Clear();
                    }
                }
            }
        }
        else if(where == "Producer")
        {
            int? id = message.Value.Id;
            if (message.Value.Id != null)
            {
                var producer = ProducerModels.FirstOrDefault(x => x.Id == id);
                foreach (var item in CatalogueModels)
                {
                    if (item.Children != null)
                    {
                        var part = item.Children;
                        foreach (var pr in part)
                        {
                            if (pr.ProducerId == id)
                            {
                                pr.ProducerId = 1;
                                pr.ProducerName = "Неизвестный";
                            }
                        }
                    }
                }

                if (producer != null)
                    ProducerModels.Remove(producer);
            }
        }
    }

    private void OnDataBaseEdited(object recipient, EditedMessage message)
    {
        var where = message.Value.Where;
        if (where == "PartCatalogue")
        {
            var uniId = message.Value.Id;
            var model = (CatalogueModel?)message.Value.What;
            if (model != null)
            {
                var item = CatalogueModels.SingleOrDefault(x => x.UniId == uniId);
                if (item != null) CatalogueModels.ReplaceOrAdd(item, model);
            }
        }
        else if (where == "Currencies")
        {
            var what = message.Value.What as IEnumerable<CurrencyModel>;
            if (what != null)
            {
                CurrencyModels.Clear();
                CurrencyModels.AddRange(what);
            }
        }
        else if (where == "CataloguePrices")
        {
            var what = (CatalogueModel?)message.Value.What;
            if (what != null)
            {
                var mainName = CatalogueModels.SingleOrDefault(x => x.UniId == what.UniId);
                if (mainName != null && mainName.Children != null)
                {
                    var mainCats = mainName.Children.SingleOrDefault(x => x.MainCatId == what.MainCatId);
                    if (mainCats != null) mainName.Children.ReplaceOrAdd(mainCats, what);
                }
            }
        }
        else if (where == "CataloguePricesList")
        {
            var what = (IEnumerable<CatalogueModel>?)message.Value.What;
            if (what != null)
                foreach (var item in what)
                {
                    var mainName = CatalogueModels.Single(x => x.UniId == item.UniId);
                    if (mainName.Children != null)
                    {
                        var mainCats = mainName.Children.SingleOrDefault(x => x.MainCatId == item.MainCatId);
                        if (mainCats != null) mainName.Children.ReplaceOrAdd(mainCats, item);
                    }
                }
        }
        else if (where == "Producer")
        {
            var id = message.Value.Id;
            var newName = message.Value.MainName;
            
            foreach (var item in CatalogueModels)
            {
                if (item.Children != null)
                {
                    foreach (var chld in item.Children)
                    {
                        if (chld.ProducerId == id)
                            chld.ProducerName = newName;
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
        CatalogueModels.Clear();
        CatalogueModels.AddRange(await _topModel.GetCatalogueAsync());

        ProducerModels.Clear();
        ProducerModels.AddRange(await _topModel.GetProducersAsync());

        AgentModels.Clear();
        AgentModels.AddRange(await _topModel.GetAllAgentsAsync());

        CurrencyModels.Clear();
        CurrencyModels.AddRange(await _topModel.GetAllCurrenciesAsync());
    }
}