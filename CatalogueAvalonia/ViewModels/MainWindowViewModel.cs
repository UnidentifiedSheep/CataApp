﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CatalogueAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly TopModel _topModel;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenCurrencySettingsCommand))] [NotifyCanExecuteChangedFor(nameof(OpenProducersSettingsCommand))]
    private bool _isDataBaseLoaded;

    public MainWindowViewModel(IMessenger messenger, CatalogueViewModel catalogueViewModel,
        AgentViewModel agentViewModel, ZakupkaViewModel zakupkaViewModel, DataStore dataStore,
        IDialogueService dialogueService, TopModel topModel, ProdajaViewModel prodajaViewModel) : base(messenger)
    {
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;
        ViewModels =
        [
            catalogueViewModel,
            agentViewModel,
            zakupkaViewModel,
            prodajaViewModel
        ];
        Messenger.Register<ActionMessage>(this, OnDataBaseLoaded);
    }

    public ObservableCollection<object> ViewModels { get; }

    private void OnDataBaseLoaded(object recipient, ActionMessage message)
    {
        if (message.Value == "DataBaseLoaded") Dispatcher.UIThread.Post(() => IsDataBaseLoaded = true);
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task OpenCurrencySettings(Window parent)
    {
        await _dialogueService.OpenDialogue(new CurrencySettingsWindow(),
            new CurrencySettingsViewModel(Messenger, _dataStore, _topModel), parent);
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task OpenProducersSettings(Window parent)
    {
        await _dialogueService.OpenDialogue(new ProducerWindow(),
            new ProducerViewModel(Messenger, _dataStore, _topModel, _dialogueService), parent);
    }

    public IMessenger GetMessenger()
    {
        return Messenger;
    }
}