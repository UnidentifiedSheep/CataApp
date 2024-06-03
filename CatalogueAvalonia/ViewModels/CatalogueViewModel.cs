using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataStore;
using CatalogueAvalonia.Services.DialogueServices;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using CatalogueAvalonia.Views.DialogueWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.ViewModels;

//Если не использовать Deselct для treegrid то приложение может крашнутся.
public partial class CatalogueViewModel : ViewModelBase
{
    private readonly ObservableCollection<CatalogueModel> _catalogueModels;

    private readonly DataStore _dataStore;
    private readonly IDialogueService _dialogueService;
    private readonly TopModel _topModel;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddNewPartCommand))]
    private bool _isDataBaseLoaded;
    private bool _isFilteringByUniValue;

    [ObservableProperty] private bool _isLoaded = !false;

    [ObservableProperty] private string _partName = string.Empty;

    [ObservableProperty] private string _partUniValue = string.Empty;

    [ObservableProperty] private CatalogueModel? _selecteditem;
    
    [ObservableProperty] private Bitmap? _itemsImg;
    [ObservableProperty] private bool _isImgVisible;
    [ObservableProperty] private bool _isImgLoading;
    public CatalogueViewModel()
    {
    }

    public CatalogueViewModel(IMessenger messenger, DataStore dataStore,
        TopModel topModel, IDialogueService dialogueService) : base(messenger)
    {
        _dialogueService = dialogueService;
        _topModel = topModel;
        _dataStore = dataStore;
        _catalogueModels = new ObservableCollection<CatalogueModel>();
        CatalogueModels = new HierarchicalTreeDataGridSource<CatalogueModel>(_catalogueModels)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<CatalogueModel>(
                    new TextColumn<CatalogueModel, string>
                        ("Название", x => x.Name, new GridLength(580)), x => x.Children),
                new TextColumn<CatalogueModel, string>(
                    "Номер запчасти", x => x.UniValue, new GridLength(150)),
                new TextColumn<CatalogueModel, string>(
                    "Производитель", x => x.ProducerName, new GridLength(130)),
                new TextColumn<CatalogueModel, decimal>(
                    "Цена", x => x.VisiblePrice, GridLength.Star),
                new TextColumn<CatalogueModel, int>(
                    "Количество", x => x.Count, GridLength.Star)
            }
        };
        Messenger.Register<ActionMessage>(this, OnAction);
        Messenger.Register<EditedMessage>(this, OnEditedIdDataBase);
        Messenger.Register<DeletedMessage>(this, OnDataBaseDeleted);
        Messenger.Register<AddedMessage>(this, OnDataBaseAdded);
        
        CatalogueModels.RowSelection!.SelectionChanged += CatalogueModelSelectionChanged;
    }

    private void CatalogueModelSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<CatalogueModel> e)
    {
        Selecteditem = CatalogueModels.RowSelection!.SelectedItem;
        if (Selecteditem != null && Selecteditem.MainCatId != null)
        {
            GetImageCommand.Execute(null);
        }
        else
        {
            if (ItemsImg != null)
                ItemsImg.Dispose();
            
        }
    }

    [RelayCommand]
    private async Task GetImage()
    {
        IsImgLoading = true;
        
        if (Selecteditem != null && Selecteditem.MainCatId != null)
        {
            if (ItemsImg != null)
                ItemsImg.Dispose();
            
            ItemsImg = await _topModel.GetPartsImg(Selecteditem.MainCatId);
            if (ItemsImg != null)
                IsImgVisible = true;
            else
                IsImgVisible = false;
        }
        else
        {
            if (ItemsImg != null)
            {
                ItemsImg.Dispose();
                IsImgVisible = false;
                ItemsImg = null;
            }
            else
            {
                IsImgVisible = false;
                ItemsImg = null;
            }
        }

        IsImgLoading = false;
    }

    [RelayCommand(CanExecute = nameof(CanDeletePart))]
    private async Task SetImgsPart(Window parent)
    {
        if (Selecteditem != null && Selecteditem.MainCatId != null)
        {
            await _dialogueService.OpenDialogue(new ImgDragAndDropWindow(),
                new ImgDragAndDropViewModel(Messenger, _topModel, Selecteditem.MainCatId), parent);
        }
        GetImageCommand.Execute(null);
    }

    private int _imgCount = 0;
    [RelayCommand]
    private async Task OpenImageInDialogue(Window parent)
    {
        if (_imgCount >= 10)
        {
            _imgCount = 0;
            DirectoryInfo di = new DirectoryInfo("../Documents");
            var files = di.GetFiles();
            files[0].Delete();
        }
        if (ItemsImg != null)
        {
            var dt = DateTime.Now.ToString("h.mm");
            IsImgLoading = true;
            await Task.Run(() => ItemsImg.Save($"../Documents/PartImg({dt}).png"));
            IsImgLoading = false;
            var a = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = a.Substring(0, a.LastIndexOf('\\'));
            path = path.Substring(0, path.LastIndexOf('\\')) + $@"\Documents\PartImg({dt}).png";
            
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception e)
            {
                await MessageBoxManager.GetMessageBoxStandard("Не удалось открыть изображение",
                    $"{e}").ShowWindowDialogAsync(parent);
            }

            _imgCount++;
        }
    }

    public HierarchicalTreeDataGridSource<CatalogueModel> CatalogueModels { get; }

    private void OnAction(object recipient, ActionMessage message)
    {
        if (message.Value == "DataBaseLoaded")
            Dispatcher.UIThread.Post(() =>
            {
                IsLoaded = !true;
                IsDataBaseLoaded = true;
                _catalogueModels.AddRange(_dataStore.CatalogueModels);
            });
    }

    private void OnDataBaseAdded(object recipient, AddedMessage message)
    {
        if (message.Value.Where == "Catalogue")
        {
            var what = message.Value.What as CatalogueModel;
            if (what != null)
                Dispatcher.UIThread.Post(() =>
                {
                    _catalogueModels.Add(what);
                });
        }
    }

    private void OnDataBaseDeleted(object recipient, DeletedMessage message)
    {
        var where = message.Value.Where;
        if (where == "PartCatalogue")
            Dispatcher.UIThread.Post(() =>
                _catalogueModels.Remove(_catalogueModels.Single(x => x.UniId == message.Value.Id)));
    }

    private void OnEditedIdDataBase(object recipient, EditedMessage message)
    {
        var where = message.Value.Where;
        if (where == "PartCatalogue")
        {
            var uniId = message.Value.Id;
            var model = (CatalogueModel?)message.Value.What;
            if (model != null)
            {
                var item = _catalogueModels.SingleOrDefault(x => x.UniId == uniId);
                if (item != null)
                    Dispatcher.UIThread.Post(() =>
                    {
                        _catalogueModels.ReplaceOrAdd(item, model);
                    });
            }
        }
    }

    partial void OnPartNameChanged(string value)
    {
        var filter = new AsyncRelayCommand(async token =>
        {
            _catalogueModels.Clear();
            await foreach (var res in DataFiltering.FilterByMainName(_dataStore.CatalogueModels, value, token))
                _catalogueModels.Add(res);
        });
        if (_isFilteringByUniValue)
        {
        }
        else if (value.Length >= 3)
        {
            filter.Execute(null);
        }
        else if (value.Length <= 2 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
        {
            _catalogueModels.Clear();
            _catalogueModels.AddRange(_dataStore.CatalogueModels);
        }
    }

    partial void OnPartUniValueChanged(string value)
    {
        var filter = new AsyncRelayCommand(async token =>
        {
            _catalogueModels.Clear();
            await foreach (var res in DataFiltering.FilterByUniValue(_dataStore.CatalogueModels, value, token))
                _catalogueModels.Add(res);
        });
        if (value.Length >= 2)
        {
            filter.Execute(null);
            _isFilteringByUniValue = true;
        }
        else if (value.Length <= 1 && _catalogueModels.Count != _dataStore.CatalogueModels.Count)
        {
            _isFilteringByUniValue = false;
            _catalogueModels.Clear();
            _catalogueModels.AddRange(_dataStore.CatalogueModels);
            OnPartNameChanged(PartName);
        }
    }

    private bool CanDeleteGroup()
    {
        return Selecteditem != null && Selecteditem.UniId != null && Selecteditem.MainCatId == null &&
               Selecteditem.UniId != 5923;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteGroup))]
    private async Task DeleteGroup(Window parent)
    {
        if (Selecteditem != null)
        {
            var res = await MessageBoxManager.GetMessageBoxStandard("Удалить группу запчастей",
                $"Вы уверенны что хотите удалить: \n\"{Selecteditem.Name}\"?",
                ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
            if (res == ButtonResult.Yes)
            {
                await _topModel.DeleteGroupFromCatalogue(Selecteditem.UniId);
                _catalogueModels.Remove(Selecteditem);
                _dataStore.CatalogueModels.Remove(Selecteditem);
            }
        }
    }

    private bool CanDeletePart() => Selecteditem != null && Selecteditem.MainCatId != null && Selecteditem.UniId != null &&
                                    Selecteditem.UniId != 5923;
    

    [RelayCommand(CanExecute = nameof(CanDeletePart))]
    private async Task DeleteSolo(Window parent)
    {
        if (Selecteditem != null)
        {
            var res = await MessageBoxManager.GetMessageBoxStandard("Удалить запчасть",
                $"Вы уверенны что хотите удалить: \n\"{Selecteditem.UniValue}\"?",
                ButtonEnum.YesNo).ShowWindowDialogAsync(parent);
            if (res == ButtonResult.Yes)
            {
                var model = _catalogueModels.SingleOrDefault(x => x.UniId == Selecteditem.UniId);

                if (model != null)
                {
                    await _topModel.DeleteSoloFromCatalogue(Selecteditem.MainCatId);
                    var item = _dataStore.CatalogueModels.SingleOrDefault(x => x.UniId == Selecteditem.UniId);
                    if (item != null && item.Children != null)
                    {
                        item.Children.Remove(Selecteditem);
                    }
                    GetImageCommand.Execute(null);
                }
            }
        }
    }

    private bool CanEditPrices() => Selecteditem != null && Selecteditem.MainCatId != null && Selecteditem.UniId != 5923;
    

    [RelayCommand(CanExecute = nameof(CanEditPrices))]
    private async Task EditPrices(Window parent)
    {
        if (Selecteditem != null)
        {
            await _dialogueService.OpenDialogue(new EditPricesWindow(),
                new EditPricesViewModel(Messenger, _topModel, Selecteditem.MainCatId ?? default, _dataStore,
                    Selecteditem.UniValue), parent);
            CatalogueModels.RowSelection!.Deselect(CatalogueModels.RowSelection.SelectedIndex);
            GetImageCommand.Execute(null);
        }
    }

    private bool CanEditCatalogue() => Selecteditem != null && Selecteditem.UniId != null && Selecteditem.UniId != 5923;
    

    [RelayCommand(CanExecute = nameof(CanEditCatalogue))]
    private async Task EditCatalogue(Window parent)
    {
        if (Selecteditem != null)
        {
            await _dialogueService.OpenDialogue(new EditCatalogueWindow(),
                new EditCatalogueViewModel(Messenger, _dataStore, Selecteditem.UniId, _topModel), parent);
            CatalogueModels.RowSelection!.Deselect(CatalogueModels.RowSelection.SelectedIndex);
            GetImageCommand.Execute(null);
        }
        
    }

    [RelayCommand(CanExecute = nameof(IsDataBaseLoaded))]
    private async Task AddNewPart(Window parent)
    {
        await _dialogueService.OpenDialogue(new AddNewPartView(),
            new AddNewPartViewModel(Messenger, _dataStore, _topModel), parent);
        CatalogueModels.RowSelection!.Deselect(CatalogueModels.RowSelection.SelectedIndex);
        GetImageCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(CanDeletePart))]
    private async Task ChangeColor(Window parent)
    {
        if (Selecteditem != null)
        {
            await _dialogueService.OpenDialogue(new EditColorWindow(),
                new EditColorViewModel(Messenger, Selecteditem.RowColor, Selecteditem.TextColor, Selecteditem.UniValue, Selecteditem.MainCatId ?? default, _topModel), parent);
        }
    }
}