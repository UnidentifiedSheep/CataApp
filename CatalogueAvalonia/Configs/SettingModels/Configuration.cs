namespace CatalogueAvalonia.Configs.SettingModels;

public class Configuration
{
    public SettingModel Settings;
    
    public Configuration(OpenAndReadConfig reader)
    {
        Settings = reader.Settings;
    }
}