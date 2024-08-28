using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.DialogueViewModel;

public class SettingsViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly ObservableCollection<SettingModel> _settings;
    public SettingsViewModel() { }

    public SettingsViewModel(IMessenger messenger, Configuration config) : base(messenger)
    {
        _configuration = config;
        _settings = new ObservableCollection<SettingModel>(_configuration.Settings.Children);

        Settings = new HierarchicalTreeDataGridSource<SettingModel>(_settings)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<SettingModel>(
                    new TextColumn<SettingModel, string>
                        ("property", x => x.SettingName, new GridLength(300)), x => x.Children)
            }
        };
    }
    public HierarchicalTreeDataGridSource<SettingModel> Settings { get; }
}