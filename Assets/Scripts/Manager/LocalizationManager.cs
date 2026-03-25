using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameLanguage
{
    English,
    Vietnamese
}

public class LocalizationManager : Singleton<LocalizationManager>
{
    public event Action OnLanguageChanged;
    
    // Dictionary to hold language data
    private Dictionary<string, string> localizedText;
    private GameLanguage currentLanguage;
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

    protected override void Awake()
    {
        base.Awake();

        // If this is a duplicate that wasn't destroyed yet, skip init
        if (Instance != this) return;

        localizedText = new Dictionary<string, string>();
        
        // Load the saved language or default to English
        string savedLangStr = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, GameLanguage.English.ToString());
        if (Enum.TryParse(savedLangStr, out GameLanguage savedLang))
        {
            currentLanguage = savedLang;
        }
        else
        {
            currentLanguage = GameLanguage.English;
        }

        // Load data only, do NOT fire OnLanguageChanged yet
        LoadLanguageData(currentLanguage);

        // Subscribe to scene loaded, so we can re-notify new LocalizedText on every scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called by Unity after every scene finishes loading
    // Re-fires the event so all new LocalizedText components in the new scene update themselves
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnLanguageChanged?.Invoke();
    }

    // Start runs after all Awakes on the FIRST scene - safe to notify subscribers now
    private void Start()
    {
        OnLanguageChanged?.Invoke();
    }

    private string GetLanguageCode(GameLanguage lang)
    {
        switch (lang)
        {
            case GameLanguage.Vietnamese: return "vi";
            case GameLanguage.English:
            default:
                return "en";
        }
    }

    /// <summary>
    /// Internal: load JSON into dictionary WITHOUT firing OnLanguageChanged.
    /// </summary>
    private bool LoadLanguageData(GameLanguage lang)
    {
        string langCode = GetLanguageCode(lang);
        TextAsset targetFile = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (targetFile == null)
        {
            Debug.LogError($"[LocalizationManager] Cannot find language file: Localization/{langCode}.json in Resources");
            return false;
        }

        LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(targetFile.text);
        if (loadedData == null || loadedData.items == null)
        {
            Debug.LogError($"[LocalizationManager] Failed to parse JSON: Localization/{langCode}.json");
            return false;
        }

        localizedText.Clear();
        foreach (var item in loadedData.items)
        {
            if (!string.IsNullOrEmpty(item.key) && !localizedText.ContainsKey(item.key))
            {
                localizedText.Add(item.key, item.value);
            }
        }

        currentLanguage = lang;
        return true;
    }

    /// <summary>
    /// Load language JSON file from Resources/Localization folder.
    /// </summary>
    public void LoadLanguage(GameLanguage lang)
    {
        if (LoadLanguageData(lang))
        {
            PlayerPrefs.SetString(LANGUAGE_PREF_KEY, currentLanguage.ToString());
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    /// <summary>
    /// Switch to a new language at runtime.
    /// </summary>
    public void ChangeLanguage(GameLanguage newLang)
    {
        if (currentLanguage == newLang) return;
        LoadLanguage(newLang);
    }

    /// <summary>
    /// Helper for Unity UI Button OnClick events (String parameter).
    /// </summary>
    public void ChangeLanguage(string langName)
    {
        if (Enum.TryParse(langName, true, out GameLanguage parsedLang))
        {
            ChangeLanguage(parsedLang);
        }
        else
        {
            Debug.LogError($"[LocalizationManager] Invalid enum string from button: {langName}");
        }
    }

    /// <summary>
    /// Helper for Unity UI Button OnClick events (Int parameter).
    /// </summary>
    public void ChangeLanguage(int langIndex)
    {
        if (Enum.IsDefined(typeof(GameLanguage), langIndex))
        {
            ChangeLanguage((GameLanguage)langIndex);
        }
    }

    /// <summary>
    /// Retrieve the localized string.
    /// </summary>
    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        
        if (localizedText != null && localizedText.TryGetValue(key, out string result))
        {
            return result;
        }

        Debug.LogWarning($"[LocalizationManager] Key not found: {key}");
        return $"[{key}]";
    }

    public GameLanguage GetCurrentLanguage()
    {
        return currentLanguage;
    }
}
