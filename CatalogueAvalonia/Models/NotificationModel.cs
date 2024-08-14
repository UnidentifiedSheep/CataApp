using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class NotificationModel : ObservableObject
{
    public int FileId { get; set; }
    [ObservableProperty] private Bitmap? _ico;
    [ObservableProperty] private FileStatus _statusOfFile;
    [ObservableProperty] private string _fileInfo = string.Empty;
    [ObservableProperty] private int _currStep;
    [ObservableProperty] private int _totalSteps;
    [ObservableProperty] private string _stepsState = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DescriptionModel? Description { get; set; }

    partial void OnCurrStepChanged(int value) => StepsState = $"{value}/{TotalSteps}";
    partial void OnTotalStepsChanged(int value) => StepsState = $"{CurrStep}/{value}";
    
}

public enum FileStatus
{
    Processing = 0,
    Ready = 1,
    NotAvailable = 2
}