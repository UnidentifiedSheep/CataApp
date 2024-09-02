using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CatalogueAvalonia.ViewModels.SettingsViewModels;

public partial class FontNormalViewModel : ViewModelBase
{
    private ObservableCollection<FontFamily> _fonts = new();
    public IEnumerable<FontFamily> Fonts => _fonts;
    [ObservableProperty] private SettingModel _thisSetting;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _fontSize = 1;
    [ObservableProperty] private Color? _foreColor;
    [ObservableProperty] private string? _foreGround = string.Empty;
    [ObservableProperty] private FontFamily _selectedFont;

    [ObservableProperty] private string _testString =
        "Это тестовый текст для проверки настроек.\n Размера шрифта, самого шрифта и его цвета."+
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt \n" +
        "ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco\n" +
        " laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate \n" +
        "velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, \n" +
        "sunt in culpa qui officia deserunt mollit anim id est laborum.";
    public FontNormalViewModel()
    {
        _fonts.AddRange(FontManager.Current.SystemFonts);
        SelectedFont = FontFamily.Default;
        Description = "Это описание";
    }
    public FontNormalViewModel(IMessenger messenger, SettingModel settingModel) : base(messenger)
    {
        _thisSetting = settingModel;
        _fonts.AddRange(FontManager.Current.SystemFonts);
        Messenger.Register<ActionMessage>(this, OnRegistered);
    }
    partial void OnForeColorChanged(Color? value)
    {
        ForeGround = value.ToString();
    }

    private void OnRegistered(object recipient, ActionMessage message)
    {
        if (message.Value.Value == "ConfigurationLoaded")
        {
            Description = ThisSetting.ValuePairs.First(x => x.Key.ToLower().Contains("description")).Value;
            FontSize = Convert.ToInt32(ThisSetting.ValuePairs.First(x => x.Key.ToLower().Contains("fontsize")).Value);

            Color.TryParse(ThisSetting.ValuePairs.First(x => x.Key.ToLower().Contains("foreground")).Value, out var color);
            ForeColor = color;
            var font = ThisSetting.ValuePairs.First(x => x.Key.ToLower().Contains("fontfamily")).Value;
            SelectedFont = _fonts.First(x => x.Name.ToLower().Contains(font.ToLower()));
        } 
    }
}