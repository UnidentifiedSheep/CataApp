using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class EditProdajaViewModel : ViewModelBase
{
    private readonly TopModel _topModel;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly ObservableCollection<CurrencyModel> _currencies;
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly ObservableCollection<ProdajaAltModel> _prodajaAlts;
    private readonly ProdajaModel _prodajaModel;
    
    public IEnumerable<CurrencyModel> Currencies => _currencies;
    public IEnumerable<AgentModel> Agents => _agents;
    public IEnumerable<ProdajaAltModel> Prodaja => _prodajaAlts;
    
    private List<Tuple<int, double>> _deletedIds = new List<Tuple<int, double>>();
    private Dictionary<int, int> _prevCounts = new Dictionary<int, int>();
    public bool IsDirty = false;
    
    [ObservableProperty]
    private DateTime _prodajaDate;
    [ObservableProperty]
    private double _totalSum;
    [ObservableProperty]
    private AgentModel? _selectedAgent;
    [ObservableProperty]
    private CurrencyModel? _selectedCurrency;
    [ObservableProperty]
    private ProdajaAltModel? _selectedProdaja;
    [ObservableProperty]
    private bool _isVisibleConverter = true;
    [ObservableProperty]
    string _comment = string.Empty;
    [ObservableProperty] 
    private int _overPrice = 20;
    
    public EditProdajaViewModel() 
    {
        _currencies = new ObservableCollection<CurrencyModel>();
        _agents = new ObservableCollection<AgentModel>();
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();
        _prodajaDate = DateTime.Now.Date;
    }
    public EditProdajaViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel, IDialogueService dialogueService, ProdajaModel prodMainGroup) : base(messenger)
    {
        _prodajaModel = prodMainGroup;
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        _comment = prodMainGroup.Comment ?? String.Empty;
        DateTime.TryParse(prodMainGroup.Datetime, out _prodajaDate);
        _currencies = new ObservableCollection<CurrencyModel>(_dataStore.CurrencyModels.Where(x => x.Id != 1));
        _agents = new ObservableCollection<AgentModel>(_dataStore.AgentModels.Where(x => x.IsZak == 1 && x.Id != 1));
        _prodajaAlts = new ObservableCollection<ProdajaAltModel>();
        Messenger.Register<AddedMessage>(this, OnItemAdded);

        LoadProdajaCommand.Execute(null);
    }

    [RelayCommand]
    public async Task LoadProdaja()
    {
        _prodajaAlts.AddRange(await _topModel.GetProdajaAltGroupAsync(_prodajaModel.Id));
        
        foreach (var item in _prodajaAlts)
        {
            var mainCatPrices = await _topModel.GetMainCatPricesByIdAsync(item.MainCatId ?? 5923);
            item.MaxCount = item.Count + mainCatPrices.Sum(x => x.Count);
            if (!_prevCounts.ContainsKey(item.MainCatId ?? 0))
            {
                _prevCounts.Add(item.MainCatId ?? 0, item.Count);
            }
            else
            {
                _prevCounts[item.MainCatId ?? 0] += item.Count;
            }
        }
        
        if (_currencies.Any(x => x.Id == _prodajaModel.CurrencyId))
            SelectedCurrency = _currencies.First(x => x.Id == _prodajaModel.CurrencyId);
        if (_agents.Any(x => x.Id == _prodajaModel.AgentId))
            SelectedAgent = _agents.First(x => x.Id == _prodajaModel.AgentId);
        TotalSum = Math.Round(_prodajaAlts.Sum(x => x.PriceSum), 2);

        IsDirty = false;
    }
    private void OnItemAdded(object recipient, AddedMessage message)
    {
        var where = message.Value.Where;
        if (where == "CataloguePartItemSelected")
        {
            var what = (CatalogueModel?)message.Value.What;
            string mainName = message.Value.MainName;
            if (what != null)
            {
                _prodajaAlts.Add(new ProdajaAltModel()
                {
                    Id = null,
                    MainCatId = what.MainCatId,
                    MainCatName = what.Name,
                    MainName = mainName,
                    UniValue = what.UniValue,
                    ProdajaId = _prodajaModel.Id
                }) ;
                IsDirty = true;
            }
        }
        else if (where == "ZakupkaPartItemEdited")
        {
            var what = (CatalogueModel?)message.Value.What;
            string mainName = message.Value.MainName;
            if (what != null)
            {
                if (SelectedProdaja != null)
                {
                    if (SelectedProdaja.Id != null && !_deletedIds.Any(x => x.Item1 == SelectedProdaja.Id))
                    {
                        _deletedIds.Add(new Tuple<int, double>(SelectedProdaja.Id ?? 0, SelectedProdaja.Price));
                    }
                    SelectedProdaja.Id = null;
                    SelectedProdaja.MainCatId = what.MainCatId;
                    SelectedProdaja.MainCatName = what.Name;
                    SelectedProdaja.MainName = mainName;
                    SelectedProdaja.UniValue = what.UniValue;
                }
                IsDirty = true;
            }
        }
    }
    partial void OnProdajaDateChanged(DateTime value)
    {
        IsDirty = true;
    }
    partial void OnSelectedCurrencyChanged(CurrencyModel? value)
    {
        if (value != null)
        {
            IsDirty = true;
        }
    }
    [RelayCommand]
    private async Task AddNewPart(Window parent)
    {
        SelectedProdaja = null;
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 0), parent);
    }
    [RelayCommand]
    private void DeletePart()
    {
        if (SelectedProdaja != null)
        {
            if (SelectedProdaja.Id != null)
            {
                _deletedIds.Add(new Tuple<int, double>(SelectedProdaja.Id ?? 0, SelectedProdaja.Price));
            }
            _prodajaAlts.Remove(SelectedProdaja);
            IsDirty = true;
        }
        TotalSum = Math.Round(_prodajaAlts.Sum(x => x.PriceSum), 2);
    }
    [RelayCommand]
    private async Task ChangePart(Window parent)
    {
        await _dialogueService.OpenDialogue(new CatalogueItemWindow(), new CatalogueItemViewModel(Messenger, _dataStore, 1), parent);
			
    }
    partial void OnCommentChanged(string value)
    {
        IsDirty = true;
    }
    public void RemoveWhereZero(IEnumerable<ProdajaAltModel> altModels)
    {
        _prodajaAlts.RemoveMany(altModels);
    }
    [RelayCommand]
    private async Task SaveZakupka()
    {
        if (SelectedCurrency != null)
        {
            var catas = await _topModel.EditProdajaAsync(_deletedIds, Prodaja, _prevCounts, SelectedCurrency, 
                Converters.ToDateTimeSqlite(ProdajaDate.Date.ToString("dd.MM.yyyy")), Math.Round(TotalSum, 2), _prodajaModel.TransactionId, Comment);
            
            Messenger.Send(new EditedMessage(new ChangedItem { Where = "CataloguePricesList", What = catas }));
            Messenger.Send(new ActionMessage("Update"));
        }
    }
    [RelayCommand]
    private async Task DeleteAll()
    {
        IEnumerable<CatalogueModel> catas = new List<CatalogueModel>();
				
        catas = await _topModel.DeleteProdajaAsync(_prodajaModel.TransactionId, _prodajaAlts, SelectedCurrency.Id ?? 0);

        Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
        Messenger.Send(new ActionMessage("Update"));
			
    }
}