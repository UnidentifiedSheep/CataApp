using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.ViewModels.ItemViewModel;
using CatalogueAvalonia.Views;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting.Internal;

namespace CatalogueAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly TopModel _topModel;
    private readonly Configuration _configuration;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenCurrencySettingsCommand))] [NotifyCanExecuteChangedFor(nameof(OpenProducersSettingsCommand))]
    private bool _isDataBaseLoaded;

    [ObservableProperty] private bool _isVisAndEnb = false;
    [ObservableProperty] private string _fastSearch = string.Empty;

    public MainWindowViewModel(IMessenger messenger, CatalogueViewModel catalogueViewModel, Configuration configuration,
        AgentViewModel agentViewModel, ZakupkaViewModel zakupkaViewModel, DataStore dataStore,
        IDialogueService dialogueService, TopModel topModel, ProdajaViewModel prodajaViewModel, FileAndNotificationsViewModel fileAndNotificationsViewModel,
        SettingsViewModel settingsViewModel) : base(messenger)
    {
        _dataStore = dataStore;
        _configuration = configuration;
        _topModel = topModel;
        _dialogueService = dialogueService;
        ViewModels =
        [
            catalogueViewModel,
            agentViewModel,
            zakupkaViewModel,
            prodajaViewModel,
            fileAndNotificationsViewModel,
            settingsViewModel
        ];
        Messenger.Register<ActionMessage>(this, OnDataBaseLoaded);
    }

    public ObservableCollection<object> ViewModels { get; }

    public void ChangeUniValuesVis()
    {
        var cata = (CatalogueViewModel)ViewModels[0];
        cata.UnVisCatalogue = false;
    }
    private void OnDataBaseLoaded(object recipient, ActionMessage message)
    {
        if (message.Value.Value == "DataBaseLoaded") Dispatcher.UIThread.Post(() => IsDataBaseLoaded = true);
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task OpenCurrencySettings(Window parent)
    {
        await _dialogueService.OpenDialogue(new CurrencySettingsWindow(),
            new CurrencySettingsViewModel(Messenger, _dataStore, _topModel), parent);
    }
    [RelayCommand]
    private async Task OpenMainSettings(Window parent)
    {
        await _dialogueService.OpenDialogue(new SettingsWindow(),
            new SettingsViewModel(Messenger, _configuration), parent);
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task OpenProducersSettings(Window parent)
    {
        await _dialogueService.OpenDialogue(new ProducerWindow(),
            new ProducerViewModel(Messenger, _dataStore, _topModel, _dialogueService), parent);
    }

    [RelayCommand]
    private async Task TryMakeNewPurchase(string jsonData)
    {
        var jsonArray = await DataFiltering.FromJsonToArray(jsonData);
        List<ZakupkaAltModel> models = new List<ZakupkaAltModel>();
        if (jsonArray != null)
        {
            Regex reg = new(@"[^a-zА-Яа-яA-Z0-9_]+");
            Regex regPrice = new(@"[^0-9.,]+");
            foreach (var item in jsonArray)
            {
                var count = Convert.ToInt32(item["count"]!.ToString());
                var producer = item["producer"]!.ToString();
                var uniValue = item["uniValue"]!.ToString();
                var price = Convert.ToDecimal(regPrice.Replace(item["price"]!.ToString(),""));
                if (producer == "DT") producer = "DIESEL TECHNIC";
                ProducerModel? producerModel = _dataStore.ProducerModels.FirstOrDefault(x =>
                    reg.Replace(x.ProducerName, "").ToLower().Contains(reg.Replace(producer, "").ToLower()));
                
                models.Add(new ZakupkaAltModel {Count = count,TextCount = count.ToString(), Price = price, UniValue = uniValue, ProducerModel = producerModel});
                
            }
            var mainWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
            await _dialogueService.OpenDialogue(new NewPurchaseWindow("NewPurchaseViewModel"),
                new NewPurchaseViewModel(Messenger, _dataStore, _topModel, _dialogueService, models), mainWindow);
            SetTextBoxVisOrUnvisCommand.Execute(null);
        }
    }
    [RelayCommand]
    private async Task TryEnterFullParts(string jsonData)
    {
        var jsonArray = await DataFiltering.FromJsonToArray(jsonData);
        List<CatalogueModel> models = new List<CatalogueModel>();

        if (jsonArray != null)
        { 
            Regex reg = new(@"[^a-zА-Яа-яA-Z0-9_]+");
            foreach (var item in jsonArray)
            {
                var producer = item["producer"]!.ToString();
                if (producer == "DT") producer = "DIESEL TECHNIC";
                var producers = _dataStore.ProducerModels.Where(x =>
                    reg.Replace(x.ProducerName, "").ToLower().Contains(reg.Replace(producer, "").ToLower())).ToList();
                ProducerModel? producerModel = null;
                if (producers.Any(x => x.ProducerName.ToLower() == producer.ToLower()))
                    producerModel = producers.FirstOrDefault(x => x.ProducerName.ToLower() == producer.ToLower());
                else
                    producerModel = producers.FirstOrDefault();
                if (producerModel != null)
                {
                    models.Add(new CatalogueModel { Count = 0, Name = item["name"]!.ToString(), UniValue = item["uniValue"]!.ToString(), ProducerName = producerModel.ProducerName, ProducerId = producerModel.Id});
                }
                else
                {
                    var newProducer = await _topModel.AddNewProducer(producer);
                    models.Add(new CatalogueModel { Count = 0, Name = item["name"]!.ToString(), UniValue = item["uniValue"]!.ToString(), ProducerName = newProducer!.ProducerName, ProducerId = newProducer.Id});
                    Messenger.Send(new AddedMessage(new ChangedItem { What = newProducer, Where = "Producer" }));
                }
                
            }
        }
        var mainWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
        SetTextBoxVisOrUnvisCommand.Execute(null);
        await _dialogueService.OpenDialogue(new EditCatalogueWindow(),
            new EditCatalogueViewModel(Messenger, _dataStore, _topModel, models, _dialogueService), mainWindow);
    }

    partial void OnFastSearchChanged(string value)
    {
        if (value.Length >= 2)
        {
            if (value[0] == '$' && value[1] == '$')
                TryEnterFullPartsCommand.Execute(value.Substring(2));
            else if(value[0] == '$' && value[1] == '*')
                TryMakeNewPurchaseCommand.Execute(value.Substring(2));
            
        }
    }
    [RelayCommand]
    private async Task SetTextBoxVisOrUnvis()
    {
        var mainWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
        if (IsVisAndEnb)
        {
            await mainWindow.StartTransitionDownAsync();
            IsVisAndEnb = false;
        }
        else
        {
            IsVisAndEnb = true;
            await mainWindow.StartTransitionUpAsync();
        }
        FastSearch = "";

    }
    public IMessenger GetMessenger()
    {
        return Messenger;
    }
}