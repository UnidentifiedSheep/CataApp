using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly List<SettingModel> _settingStable;
    private readonly ObservableCollection<SettingModel> _settings;
    private readonly SettingModel _mainSetting;

    [ObservableProperty] private UserControl? _currentView;
    [ObservableProperty] private ViewModelBase? _currentViewModel;
    [ObservableProperty] private string _searchBox = string.Empty;
    [ObservableProperty] private SettingModel? _selectedSetting;
    public SettingsViewModel() { }

    public SettingsViewModel(IMessenger messenger, Configuration config) : base(messenger)
    {
        _configuration = config;
        _settings = new ObservableCollection<SettingModel>(_configuration.Settings.Children);
        _settingStable = new List<SettingModel>(_configuration.Settings.Children);
        _mainSetting = _configuration.Settings;

        Settings = new HierarchicalTreeDataGridSource<SettingModel>(_settings)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<SettingModel>(
                    new TextColumn<SettingModel, string>
                        ("property", x => x.SettingName, new GridLength(300)), x => x.Children)
            }
        };
        Settings.RowSelection!.SelectionChanged += SelectionChanged;
    }

    private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<SettingModel> e)
    {
        SelectedSetting = Settings.RowSelection?.SelectedItem;
        CurrentView = null;
        CurrentViewModel = null;
        CurrentView = SelectedSetting?.View;
        CurrentViewModel = SelectedSetting?.ViewModel;
    }

    public HierarchicalTreeDataGridSource<SettingModel> Settings { get; }

    private void ExpandAll()
    {
        
    }

    partial void OnSearchBoxChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _settings.Clear();
            _settings.AddRange(_settingStable);
            return;
        }
        _settings.Clear();
        _settings.AddRange(_mainSetting.SearchViaText(value));
    }
}