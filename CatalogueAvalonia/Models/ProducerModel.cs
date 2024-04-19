using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class ProducerModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _producerName = string.Empty;

    partial void OnProducerNameChanged(string value)
    {
        if (StartName == String.Empty)
            _startName = value;
    }

    private string _startName = String.Empty;

    public string StartName
    {
        get => _startName;
        set => _startName = value;
    }
}