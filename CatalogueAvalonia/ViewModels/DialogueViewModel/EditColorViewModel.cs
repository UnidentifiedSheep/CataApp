using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CatalogueAvalonia.Models;
using CatalogueAvalonia.Services.DataBaseAction;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class EditColorViewModel : ViewModelBase
{
    private readonly TopModel _topModel;
    private readonly int _id;
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private Color _selectedTextColor;
    [ObservableProperty] private string _uniValue = string.Empty;
    public EditColorViewModel()
    {
        
    }
    public EditColorViewModel(IMessenger messenger, string currentRowColor, string currentTextColor, string uniValue, int id, TopModel topModel) : base(messenger)
    {
        _topModel = topModel;
        _id = id;
        Color.TryParse(currentRowColor, out _selectedColor);
        Color.TryParse(currentTextColor, out _selectedTextColor);
        
        _uniValue = uniValue;
    }

    [RelayCommand]
    private async Task SaveChanges(Window parent)
    {
        var model = await _topModel.EditColor(SelectedColor.ToString(), SelectedTextColor.ToString(), _id);
        Messenger.Send(new EditedMessage(new ChangedItem
            { Where = "CataloguePrices", Id = _id, What = model }));
        parent.Close();
        
    }
}