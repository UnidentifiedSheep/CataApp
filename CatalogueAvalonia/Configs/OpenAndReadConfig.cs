using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform;
using CatalogueAvalonia.Configs.SettingModels;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace CatalogueAvalonia.Configs;

public partial class OpenAndReadConfig : ObservableRecipient
{
    private readonly ILogger _logger;
    public SettingModel Settings { get; private set; } 

    public OpenAndReadConfig(ILogger logger, IMessenger messenger) : base(messenger)
    {
        _logger = logger;
        Settings = new("MainSettings|Основные настройки", Messenger);
        Settings.SetName("MainSettings|Основные настройки", Language.Rus).SetId(_id);
        TryGetConfigurationCommand.Execute(null);
    }

    private bool _ranDefault = false;
    [RelayCommand]
    private async Task TryGetConfiguration()
    {
        try
        {
            ReadUserConfiguration();
        }
        catch (Exception e)
        {
            _logger.Error($"Ошибка: {e}");
            var result = await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Произошла ошибка при попытке загрузить настройки.\nЖелаете ли сбросить до стандартных?", ButtonEnum.YesNo).ShowWindowAsync();
            if (result == ButtonResult.Yes)
                _ranDefault = true;
        }

        if (_ranDefault)
        {
            try
            {
                ReadConfigDefault();
            }
            catch (Exception e)
            {
                _logger.Error($"Ошибка: {e}");
                var result = await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    $"Произошла ошибка при попытке загрузить стандартные настройки.", ButtonEnum.YesNo).ShowWindowAsync();
            }
        }

        Messenger.Send(new ActionMessage(new ActionM("ConfigurationLoaded")));
    }

    private void ReadUserConfiguration()
    {
        var cfgPath = Directory.GetCurrentDirectory() + "\\Configs\\appsettings.json";
        using var cfg = File.OpenRead(cfgPath);
        using var reader = new JsonTextReader(new StreamReader(cfg));
        var cfgArray = (JObject)JToken.ReadFrom(reader);
        ArrayRecursive(cfgArray, Settings);
    }
    private void ReadConfigDefault()
    {
        var defaultCfgPath = Directory.GetCurrentDirectory() + "\\Configs\\defaultappsettings.json";
        using var defaultCfg = File.OpenRead(defaultCfgPath);
        using var defaultReader = new JsonTextReader(new StreamReader(defaultCfg));
        var defaultCfgArray = (JObject)JToken.ReadFrom(defaultReader);
        ArrayRecursive(defaultCfgArray, Settings);
    }

    private int _id = 0;
    private void ArrayRecursive(JObject? obj, SettingModel currentSettings)
    {
        if (obj == null) return;
        foreach (var item in obj)
        {
            _id++;
            var key = item.Key;
            var value = item.Value;

            if (value is JValue jValue)
            {
                currentSettings.ValuePairs.Add(new KeyValuePair<string, string>(key, jValue.ToString(CultureInfo.CurrentCulture)));
                continue;
            }

            var model = new SettingModel(key, Messenger);
            model.SetName(key, Language.Rus).SetId(_id);
            currentSettings.Children.Add(model);
            ArrayRecursive((JObject?)value, model);
        }
    }
}