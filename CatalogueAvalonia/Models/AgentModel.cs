using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CatalogueAvalonia.Models;

public partial class AgentModel : ObservableObject
{
    Regex reg = new(@"[^0-9]+");
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int IsZak { get; set; }
    [ObservableProperty] private int? _overPrice;
    [ObservableProperty] private string _overPriceText = "0";
    partial void OnOverPriceChanged(int? value)
    {
        if (value == null)
        {
            OverPrice = 0;
            OverPriceText = "0";
        }
    }

    partial void OnOverPriceTextChanged(string value)
    {
        var val = Convert.ToInt32(reg.Replace(value, ""));
        if (val < 0)
        {
            OverPrice = 0;
            OverPriceText = "0";
        }
        else if (val > 500)
        {
            OverPrice = 500;
            OverPriceText = "500";
        }
        else
        {
            OverPrice = val;
        }
    }
}