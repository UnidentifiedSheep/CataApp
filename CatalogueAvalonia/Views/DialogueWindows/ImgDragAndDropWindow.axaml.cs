using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CatalogueAvalonia.ViewModels.DialogueViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class ImgDragAndDropWindow : Window
{
    private EventHandler<PointerPressedEventArgs> _hand;
    public ImgDragAndDropWindow()
    {
        InitializeComponent();
        SetupDnd
        (
            async d =>
            {
                if (Assembly.GetEntryAssembly()?.GetModules().FirstOrDefault()?.FullyQualifiedName is { } name &&
                    TopLevel.GetTopLevel(this) is { } topLevel &&
                    await topLevel.StorageProvider.TryGetFileFromPathAsync(new Uri(name)) is { } storageFile)
                {
                    d.Set(DataFormats.Files, new[] { storageFile });
                }
            }, 0
        );
    }

    ~ImgDragAndDropWindow()
    {
        BorderFile.PointerPressed -= _hand;
    }
    private void SetupDnd(Func<DataObject, Task> factory, int action)
    {
        async void DoDrag(object? sender, PointerPressedEventArgs e)
        {
            var dragData = new DataObject();
            await factory(dragData);

            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        }
        void DragOver(object? sender, DragEventArgs e)
            {
                e.DragEffects = DragDropEffects.Copy;

                // Only allow if the dragged data contains text or filenames.
                if (!e.Data.Contains(DataFormats.Files))
                    e.DragEffects = DragDropEffects.None;
            }

            async void Drop(object? sender, DragEventArgs e)
            {
                if (action == 0)
                {
                   var dc = (ImgDragAndDropViewModel?)DataContext;
                e.DragEffects = DragDropEffects.Copy;
                
                if (e.Data.Contains(DataFormats.Files) && dc != null)
                {
                    var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();

                    if (files.Any())
                    {
                        if (files.First() is IStorageFile file)
                        {
                            if (new [] {".png", ".jpg", ".jpeg", ".ico"}.Any(x => file.Name.ToLower().Contains(x)))
                            {
                                await using var stream = await file.OpenReadAsync();
                                if (dc.Img !=null)
                                    dc.Img.Dispose();
                                using (var tempImg = new Bitmap(stream))
                                {
                                    if (tempImg.Size.Height > 1920 || tempImg.Size.Width > 1920)
                                    {
                                        if (tempImg.Size.Height > tempImg.Size.Width)
                                            dc.Img = tempImg.CreateScaledBitmap(new PixelSize(1080, 1920));
                                        else
                                            dc.Img = tempImg.CreateScaledBitmap(new PixelSize(1920, 1080));
                                    }
                                    else
                                    {
                                        dc.Img = tempImg;
                                    }
                                }

                                dc.FilePath = file.Path.ToString().Substring(8);
                                dc.IsLoaded = true;
                                dc.IsDirty = true;
                                foreach (var item in files)
                                   item.Dispose();
                                
                                
                            }
                            else
                                await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                                    $"Изображение должно быть с расширением \".jpg\", \".png\", \".ico\" или \".jpeg\"").ShowWindowDialogAsync(this);
                        }
                    }

                        
                } 
                }
            }

            _hand = (sender, args) => DoDrag(sender, args);
            BorderFile.PointerPressed += _hand;
            
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
    }
}