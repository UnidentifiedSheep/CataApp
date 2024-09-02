using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.ViewModels.SettingsViewModels;

public class FontViewModel : ViewModelBase
{
    private readonly ObservableCollection<UserControl> _views = new();
    private readonly SettingModel _currentSetting;
    public IEnumerable<UserControl> Views => _views;
    public FontViewModel()
    {
        
    }

    public FontViewModel(IMessenger messenger, SettingModel settingModel) : base(messenger)
    {
        _currentSetting = settingModel;
        Messenger.Register<ActionMessage>(this, OnRegistered);
    }

    private void OnRegistered(object recipient, ActionMessage message)
    {
        if (message.Value.Value == "ConfigurationLoaded")
        {
            foreach (var ch in _currentSetting.Children)
            {
                if (ch.View != null)
                    _views.Add(ch.View);
            }
        }
    }
}