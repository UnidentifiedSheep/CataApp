using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class DescriptionModel : ObservableObject
{
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string? _startDate;
    [ObservableProperty] private string? _endDate;
}