using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using CatalogueAvalonia.ViewModels.DialogueViewModel;

namespace CatalogueAvalonia.Views.DialogueWindows;

public partial class ImageViewerWindow : Window
{
    private int _currMiniMapPos = 0;
    private double _leftChange = 0;
    private double _topChange = 0;
    
    public ImageViewerWindow()
    {
        InitializeComponent();
        KeyDown += _KeyDown;
        SizeChanged += _SizeChanged;
    }
    
    ~ImageViewerWindow()
    {
        KeyDown -= _KeyDown;
        SizeChanged -= _SizeChanged;
    }
    
    private void _SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var dc = (ImageViewerViewModel?)DataContext;
        if (dc != null)
        {
            dc.Scale = 1;
            dc.GridWidth = e.NewSize.Width;
            dc.GridHeight = e.NewSize.Height;

            MiniMap.Height = dc.GridHeight / 5;
            MiniMap.Width = dc.GridWidth / 5;
            
            MiniMapBorder.Height = dc.GridHeight / 5;
            MiniMapBorder.Width = dc.GridWidth / 5;

            MiniMapBorder.HorizontalAlignment = HorizontalAlignment.Center;
            MiniMapBorder.VerticalAlignment = VerticalAlignment.Center;
            
            _leftChange = 0;
            _topChange = 0;
            ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
        }
    }
    private void _KeyDown(object? sender, KeyEventArgs e)
    {
        var dc = (ImageViewerViewModel?)DataContext;
        if (dc != null)
        {
            if (dc.Scale != 1)
            {
                if (e.PhysicalKey == PhysicalKey.ArrowUp)
                {
                    _topChange += ImgGrid.Height / (dc.Scale * 2);
                    ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
                }
                else if (e.PhysicalKey == PhysicalKey.ArrowDown)
                {
                    _topChange -= ImgGrid.Height / (dc.Scale * 2);
                    ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
                }
                else if(e.PhysicalKey == PhysicalKey.ArrowLeft)
                {
                    _leftChange += ImgGrid.Width / (dc.Scale * 2);
                    ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
                
                }
                else if(e.PhysicalKey == PhysicalKey.ArrowRight)
                {
                    _leftChange -= ImgGrid.Width / (dc.Scale * 2);
                    ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
                }
            }
        }
    }

    private void MiniMap_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            switch (_currMiniMapPos)
            {
                case 0:
                {
                    MiniMap.VerticalAlignment = VerticalAlignment.Top;
                    _currMiniMapPos++;
                    break;
                }
                case 1:
                {
                    MiniMap.HorizontalAlignment = HorizontalAlignment.Right;
                    _currMiniMapPos++;
                    break;
                }
                case 2:
                {
                    MiniMap.VerticalAlignment = VerticalAlignment.Bottom;
                    _currMiniMapPos++;
                    break;
                }
                case 3:
                {
                    MiniMap.HorizontalAlignment = HorizontalAlignment.Left;
                    _currMiniMapPos = 0;
                    break;
                }
            }
        }
    }

    private void ScrollViewer_OnScrolsdnged(object? sender, ScrollChangedEventArgs e)
    {
        var scrlBar = (ScrollViewer?)sender;
        var dc = (ImageViewerViewModel?)DataContext;
        if (scrlBar != null)
        {
            if (dc != null)
            {
                /*MiniMapBorder.HorizontalAlignment = HorizontalAlignment.Left;
                MiniMapBorder.VerticalAlignment = VerticalAlignment.Top;
                double scrMaxX = scrlBar.ScrollBarMaximum.X;
                double scrMaxY = scrlBar.ScrollBarMaximum.Y;

                double scrXprc = scrlBar.Offset.X / (scrMaxX / 100);
                double scrYprc = scrlBar.Offset.Y / (scrMaxY / 100);
    
                double miniMapBorderNewX = ((MiniMap.Width / 100) * scrXprc);
                double miniMapBorderNewY = ((MiniMap.Height / 100) * scrYprc);
                
                if (dc.Scale <= 1)
                {
                    MiniMapBorder.HorizontalAlignment = HorizontalAlignment.Center;
                    MiniMapBorder.VerticalAlignment = VerticalAlignment.Center;
                }
                else if (dc.Scale > 6)
                {
                    MiniMapBorder.RenderTransform = new TranslateTransform(miniMapBorderNewX, miniMapBorderNewY);
                }
                else if (dc.Scale > 1 && dc.Scale <= 6)
                {
                    MiniMapBorder.RenderTransform = new TranslateTransform(miniMapBorderNewX, miniMapBorderNewY);
                }

                bool isWidthClose = (MiniMap.Width - miniMapBorderNewX - MiniMapBorder.Width) < 0;
                bool isHeightClose = (MiniMap.Height - miniMapBorderNewY - MiniMapBorder.Height) < 0;

                if (isWidthClose && isHeightClose)
                {
                    MiniMapBorder.RenderTransform = new TranslateTransform(0, 0);
                    MiniMapBorder.HorizontalAlignment = HorizontalAlignment.Right;
                    MiniMapBorder.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else
                {
                    if (isWidthClose)
                    {
                        MiniMapBorder.RenderTransform = new TranslateTransform(0, miniMapBorderNewY);
                        MiniMapBorder.HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    
                    if (isHeightClose)
                    {
                        MiniMapBorder.RenderTransform = new TranslateTransform(miniMapBorderNewX, 0);
                        MiniMapBorder.VerticalAlignment = VerticalAlignment.Bottom;
                    }
                    
                }*/

            }
        }
    }


    private void MainContainer_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
        var dc = (ImageViewerViewModel?)DataContext;
        if (dc != null)
        {
            //up
            if (e.Delta.Y > 0)
            {
                if (dc.Scale < 10)
                {
                    dc.Scale += 1;
                    dc.GridHeight *= 1.2;
                    dc.GridWidth *= 1.2;

                    MiniMapBorder.Height /= 1.2;
                    MiniMapBorder.Width /= 1.2;
                }
            }
            else if(e.Delta.Y < 0)//down
            {
                if (dc.Scale > 1)
                {
                    dc.Scale -= 1;
                    dc.GridHeight /= 1.2;
                    dc.GridWidth /= 1.2;
                    
                    MiniMapBorder.Height *= 1.2;
                    MiniMapBorder.Width *= 1.2;
                }
            }

            if (dc.Scale == 1)
            {
                _leftChange = 0;
                _topChange = 0;
                ImgGrid.Margin = new Thickness(_leftChange, _topChange, 0, 0);
            }
        }
        
    }
}