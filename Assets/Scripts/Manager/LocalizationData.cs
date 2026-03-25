using System.Collections.Generic;

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}

[System.Serializable]
public class LocalizationData
{
    public List<LocalizationEntry> items;
}
