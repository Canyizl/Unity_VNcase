using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LocalizationManager : MonoBehaviour
{
    public Dictionary<string, string> localizedText;
    public string currentLanguage = Constants.DEFAULT_LANGUAGE;

    public delegate void OnLanguageChanged();
    public event OnLanguageChanged LanguageChanged;
    public static LocalizationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadLanguage(currentLanguage);
    }

    public void LoadLanguage(string language)
    {
        currentLanguage = language;
        string filePath = Path.Combine(Application.streamingAssetsPath, Constants.LANGUAGE_PATH, language + Constants.JSON_FILE_EXTENTION);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            localizedText = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataAsJson);
            LanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError(Constants.LOCALIZATION_LOAD_FAILED + filePath);
        }
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        return key;
    }
}
