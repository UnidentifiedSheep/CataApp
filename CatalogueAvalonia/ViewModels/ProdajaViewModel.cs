using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.BillingService;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Kernel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.ViewModels;

public partial class ProdajaViewModel : ViewModelBase
{
    private readonly ObservableCollection<AgentModel> _agents;
    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly ObservableCollection<ProdajaAltModel> _prodajaAltGroup;
    private readonly MainInvoice _mainInvoice;

    private readonly ObservableCollection<ProdajaModel> _prodajaMainGroup;
    private readonly TopModel _topModel;

    [ObservableProperty] private DateTime _startDate;
    [ObservableProperty] private DateTime _endDate;
    [ObservableProperty] private string _searchFiled = string.Empty;
    [ObservableProperty] private string _searchFiledComment = string.Empty;
    [ObservableProperty] private bool _isAgentsDown;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(NewProdajaCommand))]
    private bool _isLoaded;

    [ObservableProperty] private AgentModel? _selectedAgent;

    [ObservableProperty] private ProdajaAltModel? _selectedAltModel;

    [ObservableProperty] private ProdajaModel? _selectedProdaja;


    public ProdajaViewModel()
    {
        _startDate = DateTime.Now.AddMonths(-1).Date;
        _endDate = DateTime.Now.Date;
        _prodajaAltGroup = new ObservableCollection<ProdajaAltModel>();
        _prodajaMainGroup = new ObservableCollection<ProdajaModel>();
        _agents = new ObservableCollection<AgentModel>();
    }

    public ProdajaViewModel(IMessenger messenger, DataStore dataStore, TopModel topModel,
        IDialogueService dialogueService, MainInvoice mainInvoice) : base(messenger)
    {
        _mainInvoice = mainInvoice;
        _isLoaded = false;
        _dataStore = dataStore;
        _topModel = topModel;
        _dialogueService = dialogueService;

        _startDate = DateTime.Now.AddMonths(-1).Date;
        _endDate = DateTime.Now.Date;

        _prodajaAltGroup = new ObservableCollection<ProdajaAltModel>();
        _prodajaMainGroup = new ObservableCollection<ProdajaModel>();
        _agents = new ObservableCollection<AgentModel>();

        Messenger.Register<ActionMessage>(this, OnDataBaseAction);
    }

    public IEnumerable<ProdajaModel> MainGroup => _prodajaMainGroup;
    public IEnumerable<ProdajaAltModel> AltGroup => _prodajaAltGroup;
    public IEnumerable<AgentModel> Agents => _agents;

    private void OnDataBaseAction(object recipient, ActionMessage message)
    {
        if (message.Value == "DataBaseLoaded")
            Dispatcher.UIThread.Post(() =>
            {
                _agents.AddRange(_dataStore.AgentModels);
                IsLoaded = true;
            });
        else if (message.Value == "Update")
            Dispatcher.UIThread.Post(() =>
            {
                LoadProdajaMainGroupCommand.Execute(null);
                LoadProdajaAltGroupCommand.Execute(null);
            });
    }

    private readonly ObservableCollection<ProdajaModel> _prodajaFilterMain = new();
    private readonly List<string> _commentValues = new();
    partial void OnSearchFiledCommentChanged(string value)
    {
        DivideComments();
        _prodajaMainGroup.Clear();
        if (!string.IsNullOrEmpty(value.TrimStart(' ')))
        {
            foreach (var comment in _commentValues.Distinct())
            {
                _prodajaMainGroup.AddRange(_prodajaFilterMain
                    .Where(x => x.Comment != null && x.Comment.Contains($" {comment} ") && !_prodajaMainGroup.Any(z => x.Id == z.Id)));
            }
            _commentValues.Clear();
        }
        else
        {
            _prodajaMainGroup.AddRange(_prodajaFilterMain);
        }
    }

    private StringBuilder _stringBuilder = new();
    private void DivideComments()
    {
        for (int i = 0; i < SearchFiledComment.Length; i++)
        {
            
            if (SearchFiledComment.Length - 1 == i && SearchFiledComment[i] != '/')
            {
                _stringBuilder.Append(SearchFiledComment[i]);
                _commentValues.Add(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
            else if (SearchFiledComment[i] != '/')
                _stringBuilder.Append(SearchFiledComment[i]);
            else
            {
                _commentValues.Add(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
        }
        _stringBuilder.Clear();
    }
    
    
    [RelayCommand]
    private async Task LoadProdajaMainGroup()
    {
        if (SelectedAgent != null)
        {
            _prodajaMainGroup.Clear();
            _prodajaFilterMain.Clear();
            var startDate = Converters.ToDateTimeSqlite(StartDate.Date.ToString("dd.MM.yyyy"));
            var endDate = Converters.ToDateTimeSqlite(EndDate.Date.ToString("dd.MM.yyyy"));
            _prodajaMainGroup.AddRange(await _topModel.GetProdajaMainGroupAsync(startDate, endDate, SelectedAgent.Id));
            _prodajaFilterMain.AddRange(_prodajaMainGroup);
            OnSearchFiledCommentChanged(SearchFiledComment);
        }
    }

    [RelayCommand]
    private async Task LoadProdajaAltGroup()
    {
        _prodajaAltGroup.Clear();
        if (SelectedProdaja != null)
            _prodajaAltGroup.AddRange(await _topModel.GetProdajaAltGroupAsync(SelectedProdaja.Id));
    }

    partial void OnEndDateChanged(DateTime value)
    {
        if (StartDate <= value)
            LoadProdajaMainGroupCommand.Execute(null);
        else
            MessageBoxManager
                .GetMessageBoxStandard("Неверные даты", "Начальная дата должна быть меньше или равна дате конца.")
                .ShowWindowDialogAsync((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!);
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (value <= EndDate)
            LoadProdajaMainGroupCommand.Execute(null);
        else
            MessageBoxManager
                .GetMessageBoxStandard("Неверные даты", "Начальная дата должна быть меньше или равна дате конца.")
                .ShowWindowDialogAsync((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!);
    }

    partial void OnSelectedAgentChanged(AgentModel? value)
    {
        LoadProdajaMainGroupCommand.Execute(null);
    }

    partial void OnSelectedProdajaChanged(ProdajaModel? value)
    {
        LoadProdajaAltGroupCommand.Execute(null);
    }
    [RelayCommand]
    private async Task FilterAgent(string value)
    {
        _agents.Clear();
        await foreach (var agent in DataFiltering.FilterAgents(_dataStore.AgentModels, value))
            _agents.Add(agent);
    }

    partial void OnSearchFiledChanged(string value)
    {
        if (value.Length >= 1)
        {
            IsAgentsDown = true;
            FilterAgentCommand.Execute(value);
        }
        else
        {
            IsAgentsDown = false;
            if (_dataStore.AgentModels.Count != _agents.Count)
            {
                _agents.Clear();
                _agents.AddRange(_dataStore.AgentModels);
            }
        }
    }

    [RelayCommand]
    private async Task NewProdaja(Window parent)
    {
        await _dialogueService.OpenDialogue(new NewProdajaWindow("NewProdajaViewModel"),
            new NewProdajaViewModel(Messenger, _topModel, _dataStore, _dialogueService), parent);
    }

    [RelayCommand]
    private async Task DeleteProdaja(Window parent)
    {
        if (SelectedProdaja != null)
        {
            var res = await MessageBoxManager.GetMessageBoxStandard("Удалить продажу?",
                $"Вы уверенны что хотите удалить закупку с номером {SelectedProdaja.Id}?",
                ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
            if (res == ButtonResult.Yes)
            {
                IEnumerable<CatalogueModel> catas = new List<CatalogueModel>();
                catas = await _topModel.DeleteProdajaAsync(SelectedProdaja.TransactionId, _prodajaAltGroup,
                    SelectedProdaja.CurrencyId);
                Messenger.Send(new EditedMessage(new ChangedItem { What = catas, Where = "CataloguePricesList" }));
                Messenger.Send(new ActionMessage("Update"));
            }
        }
    }

    private List<QInvoiceModel> _invoiceQeue = new();
    private int _fileId;
    [RelayCommand]
    private async Task CreateInvoice(Window parent)
    {
        _fileId++;
        if (SelectedProdaja != null)
        {
            var notification = new NotificationModel
            {
                FileId = _fileId,
                StatusOfFile = FileStatus.Processing,
            };
            var qModel = new QInvoiceModel
            {
                FileId = _fileId,
                InvoiceType = InvoiceType.SingleInvoice,
                NotificationModel = notification
            };
            qModel.ProdajaModels.Add(SelectedProdaja);
            _invoiceQeue.Add(qModel);
            Messenger.Send(new AddedMessage(new ChangedItem { Id = _fileId, What = notification, Where = "FileAdded" }));
            await TryRunQueu(parent);
        }
    }
    [RelayCommand]
    private async Task CreateInvoiceExcel(Window parent)
    {
        _fileId++;
        if (SelectedProdaja != null)
        {
            var notification = new NotificationModel
            {
                FileId = _fileId,
                StatusOfFile = FileStatus.Processing,
            };
            await _mainInvoice.GenerateInvoiceExcel(await _topModel.GetProdajaAltGroupNewTask(SelectedProdaja.Id), SelectedProdaja, _fileId, notification);
            Messenger.Send(new AddedMessage(new ChangedItem { Id = _fileId, What = notification, Where = "FileAdded" }));
            await TryRunQueu(parent);
        }
    }

    private async Task TryRunQueu(Window parent)
    {
        if (_invoiceQeue.Count >= 5)
        {
            await MessageBoxManager.GetMessageBoxStandard("!",
                $"Максимальное количество докуметов в очереди должно быть не более 5.").ShowWindowDialogAsync(parent);
            return;
        }
        if (!_isQueuRunnig)
            await UnqueuInvoices(parent);
        
    }

    private bool _isQueuRunnig = false;

    private async Task UnqueuInvoices(Window parent)
    {
        while (true)
        {
            _isQueuRunnig = true;
            var qInvoice = _invoiceQeue.FirstOrDefault();
            if (qInvoice == null)
            {
                _isQueuRunnig = false;
                return;
            }

            if (qInvoice.InvoiceType == InvoiceType.SingleInvoice)
            {
                _mainInvoice.GenerateInvoice(await _topModel.GetProdajaAltGroupNewTask(qInvoice.ProdajaModels.First().Id), qInvoice.ProdajaModels.First(), _fileId, qInvoice.NotificationModel!);
            }
            else if (qInvoice.InvoiceType == InvoiceType.InvoiceForPeriodMinimal)
            {
                var res = new List<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>>();
                foreach (var main in qInvoice.ProdajaModels)
                {
                    var altModel = await _topModel.GetProdajaAltGroupNewTask(main.Id);
                    res.Add(new Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>(main, altModel));
                }

                _mainInvoice.GenerateInvoiceForPeriodMinimal(res, qInvoice.StartDate ?? new DateTime(), qInvoice.EndDate ?? new DateTime(), qInvoice.FileId, qInvoice.NotificationModel!);
            }
            else if (qInvoice.InvoiceType == InvoiceType.InvoiceForPeriod)
            {
                var res = new List<Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>>();

                foreach (var main in qInvoice.ProdajaModels)
                {
                    var altModel = await _topModel.GetProdajaAltGroupNewTask(main.Id);
                    res.Add(new Tuple<ProdajaModel, IEnumerable<ProdajaAltModel>>(main, altModel));
                }

                _mainInvoice.GenerateInvoiceForPeriod(res, qInvoice.StartDate ?? new DateTime(), qInvoice.EndDate ?? new DateTime(), _fileId, qInvoice.NotificationModel!);
            }
            

            _invoiceQeue.Remove(qInvoice);
        }
    }

    [RelayCommand]
    private async Task CreateInvoiceManyMinimal(Window parent)
    {
        _fileId++;
        var notification = new NotificationModel
        {
            FileId = _fileId,
            StatusOfFile = FileStatus.Processing,
        };
        _invoiceQeue.Add(new QInvoiceModel
        {
            FileId = _fileId,
            StartDate = StartDate,
            EndDate = EndDate,
            InvoiceType = InvoiceType.InvoiceForPeriodMinimal,
            NotificationModel = notification,
            ProdajaModels = [.._prodajaMainGroup]
        });
        Messenger.Send(new AddedMessage(new ChangedItem { Id = _fileId, What = notification, Where = "FileAdded" }));
        await TryRunQueu(parent);
    }

    [RelayCommand]
    private async Task CreateInvoiceMany(Window parent)
    {
        _fileId++;
        var notification = new NotificationModel
        {
            FileId = _fileId,
            StatusOfFile = FileStatus.Processing,
        };
        _invoiceQeue.Add(new QInvoiceModel
        {
            FileId = _fileId,
            StartDate = StartDate,
            EndDate = EndDate,
            InvoiceType = InvoiceType.InvoiceForPeriod,
            NotificationModel = notification,
            ProdajaModels = [.._prodajaMainGroup]
        });
        Messenger.Send(new AddedMessage(new ChangedItem { Id = _fileId, What = notification, Where = "FileAdded" }));
        await TryRunQueu(parent);
    }

    [RelayCommand]
    private async Task EditProdaja(Window parent)
    {
        if (SelectedProdaja != null)
            await _dialogueService.OpenDialogue(new NewProdajaWindow("EditProdajaViewModel"),
                new EditProdajaViewModel(Messenger, _dataStore, _topModel, _dialogueService, SelectedProdaja), parent);
    }
}