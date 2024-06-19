using System;
using Avalonia.Controls;
using CatalogueAvalonia.Services.Messeges;
using CatalogueAvalonia.ViewModels;
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
}