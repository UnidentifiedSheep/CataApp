namespace CatalogueAvalonia.Configs.SettingModels;

public class KeyValuePair<TK, TV>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public KeyValuePair(TK key, TV value)
    {
        Key = key;
        Value = value;
    }

    public TK Key { get; private set; }
    public TV Value { get; private set; }
    
    public void ChangeKey(TK key) => Key = key;
    public void ChangeValue(TV value) => Value = value;
}