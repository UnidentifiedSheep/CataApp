using System;
using System.Collections.ObjectModel;

namespace CatalogueAvalonia.Configs.SettingModels;

public class SettingModel
{
    public int Id { get; private set; }
    private string _fullSettingName { get; set; } = string.Empty;
    public string SettingName { get; private set; } = string.Empty;
    public ObservableCollection<KeyValuePair<string, string>> ValuePairs = new();
    public ObservableCollection<SettingModel> Children = new();

    public SettingModel SetId(int id)
    {
        Id = id;
        return this;
    }

    public SettingModel SetName(string name, Language lang)
    {
        _fullSettingName = name;
        SettingName = lang switch
        {
            Language.Eng => _fullSettingName.Substring(0, _fullSettingName.IndexOf('|')),
            Language.Rus => _fullSettingName.Substring(_fullSettingName.IndexOf('|')+1),
            _ => SettingName
        };
        return this;
    }
}