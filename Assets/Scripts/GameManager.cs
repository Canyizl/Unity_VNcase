using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string playerName;
    public string currentScene;
    public string currentStoryFile;
    public int currentLineIndex;
    public int currentLanguageIndex = Constants.DEFAULT_LANGUAGE_INDEX;
    public string currentLanguage = Constants.DEFAULT_LANGUAGE;
    public string currentBackgroundImg;
    public string currentBackgroundMusic;
    public List<CharacterSaveData> currentCharacterData = new List<CharacterSaveData>();

    public bool hasStarted;
    public HashSet<string> unlockedBackgrounds = new HashSet<string>();
    public Dictionary<string, int> maxReachedLineIndices = new Dictionary<string, int>();
    public LinkedList<ExcelReader.ExcelData> historyRecords;
    public enum SaveLoadMode { None, Save, Load }
    public SaveLoadMode currentSaveLoadMode { get; set; } = SaveLoadMode.None;
    public SaveData pendingData;
    public void Save(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex);
        File.WriteAllText(path, JsonConvert.SerializeObject(pendingData, Formatting.Indented));
    }
    public void Load(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex);
        pendingData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
    }
    public string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
    public class CharacterSaveData
    {
        public string characterID;
        public float positionX;
        public string expressionName;
    }
    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public byte[] savedScreenshotData;
        public LinkedList<ExcelReader.ExcelData> savedHistoryRecords;
        public string savedBackgroundImg;
        public string savedBackgroundMusic;
        public List<CharacterSaveData> savedCharacters;
        public string savedPlayerName;
    }
    public static GameManager Instance { get; private set; }
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
}
