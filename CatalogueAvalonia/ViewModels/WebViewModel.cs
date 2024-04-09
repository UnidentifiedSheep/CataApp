using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.ViewModels;

public partial class WebViewModel : ViewModelBase
{
    [ObservableProperty] private Uri? _url = new("https://www.google.com/");
}