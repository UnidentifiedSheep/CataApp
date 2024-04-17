using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class ImageViewerViewModel : ViewModelBase
{
    private readonly Bitmap? _img;
    [ObservableProperty] private Bitmap? _visibleImg;
    [ObservableProperty] private double _gridHeight = 490;
    [ObservableProperty] private double _gridWidth = 490;
    [ObservableProperty] private bool _isImgVisible;
    [ObservableProperty] private int _scale = 1;
    public ImageViewerViewModel()
    {
        
    }
    ~ImageViewerViewModel()
    {
        if (_img != null)
            _img.Dispose();
        if (VisibleImg != null)
            VisibleImg.Dispose();
        
    }
    public ImageViewerViewModel(Bitmap? img)
    {
        _img = img;
        _visibleImg = img;
        _isImgVisible = true;
    }
}