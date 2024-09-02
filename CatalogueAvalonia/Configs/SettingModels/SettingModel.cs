using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using CatalogueAvalonia.ViewModels.SettingsViewModels;
using CatalogueAvalonia.Views.SettingsViews;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.Configs.SettingModels;

public partial class SettingModel : ObservableObject
{
    private readonly IMessenger _messenger;
    public SettingModel(string fullSettingName, IMessenger messenger)
    {
        FullSettingName = fullSettingName;
        _messenger = messenger;
        TryGetView();
    }

    private void TryGetView()
    {
        var name = FullSettingName.Substring(0, FullSettingName.IndexOf("|", StringComparison.Ordinal));
        var viewType = Type.GetType("CatalogueAvalonia.Views.SettingsViews." +
                                    $"{name}View");
        var viewModelType = Type.GetType("CatalogueAvalonia.ViewModels.SettingsViewModels." +
                                    $"{name}ViewModel");
        if (viewType != null && viewModelType != null)
        {
            _view = (UserControl?)Activator.CreateInstance(viewType);
            
            _viewModel = (ViewModelBase?)Activator.CreateInstance(viewModelType, _messenger, this);
            _view!.DataContext = _viewModel;
        }
    }
    public int Id { get; private set; }
    [ObservableProperty] private string _fullSettingName = string.Empty;
    [ObservableProperty] private string _settingName = string.Empty;
    public ObservableCollection<KeyValuePair<string, string>> ValuePairs = new();
    public ObservableCollection<SettingModel> Children = new();

    private ViewModelBase? _viewModel;
    private UserControl? _view;
    public ViewModelBase? ViewModel => _viewModel;
    public UserControl? View => _view;

    public SettingModel SetId(int id)
    {
        Id = id;
        return this;
    }

    public SettingModel SetName(string name, Language lang)
    {
        FullSettingName = name;
        SettingName = lang switch
        {
            Language.Eng => FullSettingName.Substring(0, FullSettingName.IndexOf('|')),
            Language.Rus => FullSettingName.Substring(FullSettingName.IndexOf('|')+1),
            _ => SettingName
        };
        return this;
    }

    public IEnumerable<SettingModel> SearchViaText(string value)
    {
        var res = new List<SettingModel>();
        if (ContainsName(value))
            res.Add(this);
        if (Children.Any())
        {
            foreach (var child in Children)
                res.AddRange(child.SearchViaText(value));
        }
        return res;
    }

    private bool ContainsName(string value)
    {
        value = value.ToLower();
        return FullSettingName.ToLower().Contains(value) || ValuePairs.Any(x => x.Key.ToLower().Contains(value));
    }
}