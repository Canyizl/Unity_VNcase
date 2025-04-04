using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class LM
{
    public static string GetSpeakerName(ExcelReader.ExcelData data)
    {
        string currentSpeakerName = string.Empty;
        switch (GameManager.Instance.currentLanguageIndex)
        {
            case 0:
                currentSpeakerName = ReplaceName(data.speakerName);
                break;
            case 1:
                currentSpeakerName = ReplaceName(data.englishName);
                break;
            case 2:
                currentSpeakerName = ReplaceName(data.japaneseName);
                break;
        }
        return currentSpeakerName;
    }

    public static string GetSpeakingContent(ExcelReader.ExcelData data)
    {
        string currentSpeakingContent = string.Empty;
        switch (GameManager.Instance.currentLanguageIndex)
        {
            case 0:
                currentSpeakingContent = ReplaceName(data.speakingContent);
                break;
            case 1:
                currentSpeakingContent = ReplaceName(data.englishContent);
                break;
            case 2:
                currentSpeakingContent = ReplaceName(data.japaneseContent);
                break;
        }
        return currentSpeakingContent;
    }

    public static string ReplaceName(string content)
    {
        return content.Replace(Constants.NAME_PLACEHOLDER, GameManager.Instance.playerName);
    }
}

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
            DontDestroyOnLoad(gameObject);
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
