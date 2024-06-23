using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
        
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        var dc = (MainWindowViewModel?)DataContext;
        if (dc != null)
        {
            IMessenger messenger = dc.GetMessenger();
            messenger.Send(new ActionMessage("AppClosed"));
        }
    }
    public async Task StartTransitionUpAsync()
    {
        var animation = (Animation)Resources["TransitionUpAnimation"];
        await animation.RunAsync(SearchGrid);
    }
    public async Task StartTransitionDownAsync()
    {
        var animation = (Animation)Resources["TransitionDownAnimation"];
        await animation.RunAsync(SearchGrid);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var dc = (MainWindowViewModel?)DataContext;
        if (dc != null)
        {
            dc.SetTextBoxVisOrUnvisCommand.Execute(null);
        }
    }
}