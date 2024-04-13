using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CatalogueAvalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class ImgDragAndDropViewModel : ViewModelBase
{
    private readonly TopModel _topModel;
    private readonly int? _mainCatId;
    public bool IsDirty;
    [ObservableProperty] private string _filePath = string.Empty;
    [ObservableProperty] private Bitmap? _img;
    [ObservableProperty] private bool _isLoaded;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(DeleteImgCommand))]
    private bool _canDelete;
    
    public ImgDragAndDropViewModel()
    {
        
    }
    public ImgDragAndDropViewModel(IMessenger messenger, TopModel topModel, int? mainCatId) : base(messenger)
    {
        _topModel = topModel;
        _mainCatId = mainCatId;
        _isLoaded = false;
        
        LoadImgCommand.Execute(null);
        IsDirty = false;
    }
    ~ImgDragAndDropViewModel()
    {
        if (Img != null)
            Img.Dispose();
    }

    partial void OnImgChanged(Bitmap? value)
    {
        if (value != null)
            CanDelete = true;
        else
            CanDelete = false;
    }

    [RelayCommand]
    private async Task LoadImg()
    {
        if (_mainCatId != null)
        {
            Img = await _topModel.GetPartsImg(_mainCatId);
            if (Img != null)
                IsLoaded = true;
        }
    }

    [RelayCommand]
    private async Task OpenFilePicke(Window parent)
    {
        var file = await parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Добавить изображение",
            AllowMultiple = false,
            FileTypeFilter = new FilePickerFileType[] {new("Изображения") {Patterns = ["*.png", "*.jpg", "*.jpeg", "*.ico"]}}
        });
        if (file.Count >= 1)
        {
            FilePath = file[0].Path.ToString().Substring(8);
            await using var stream = await file[0].OpenReadAsync();
                
                using (var tempImg = new Bitmap(stream))
                {
                    if (tempImg.Size.Height > 1920 || tempImg.Size.Width > 1920)
                    {
                        if (tempImg.Size.Height > tempImg.Size.Width)
                            Img = tempImg.CreateScaledBitmap(new PixelSize(1080, 1920));
                        else
                            Img = tempImg.CreateScaledBitmap(new PixelSize(1920, 1080));
                    }
                    else
                    {
                        if (Img != null)
                        {
                            Img.Dispose();
                            Img = tempImg.CreateScaledBitmap(tempImg.PixelSize);
                        }
                        else
                            Img = tempImg.CreateScaledBitmap(tempImg.PixelSize);
                    }
                }
            
            
            IsLoaded = true;
            IsDirty = true;
        }
    }
    
    
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void DeleteImg()
    {
        if (Img != null)
        {
            Img = null;
        }
        FilePath = String.Empty;
        IsLoaded = false;
        IsDirty = true;
    }

    [RelayCommand]
    private async Task SaveChanges(Window parent)
    {
        using (var stream = new MemoryStream())
        {
            if (IsDirty)
            {
                if (Img != null)
                {
                    Img.Save(stream, 100);
                    await _topModel.SetPartsImg(_mainCatId, stream.ToArray());
                }
                else
                    await _topModel.SetPartsImg(_mainCatId, null);
            }
        }

        
        parent.Close();
    }

    [RelayCommand]
    private void Cancle(Window parent)
    {
        parent.Close();
    }
}