using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VNManager : MonoBehaviour
{
    #region Variables
    public GameObject gamePanel;
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerName;
    public TypewritterEffect typewritterEffect;
    public ScreenShotter screenShotter;

    public Image avatarImage;
    public AudioSource vocalAudio;
    public Image backgroundImage;
    public AudioSource backgroundMusic;
    public Image characterImage1;
    public Image characterImage2;

    public GameObject choicePanel;
    public Button choiceButton1;
    public Button choiceButton2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingsButton;
    public Button homeButton;
    public Button closeButton;

    private string storyPath = Constants.STORY_PATH;
    private readonly string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private readonly int defaultStartLine = Constants.DEFAULT_START_LINE;
    private readonly string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;

    private string saveFolderPath;
    private byte[] screenshotData;
    private string currentSpeakingContent;

    private List<ExcelReader.ExcelData> storyData;
    private int currentLine = Constants.DEFAULT_START_LINE;
    private string currentStoryFileName;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;

    private bool isAutoPlay = false;
    private bool isSkip = false;
    private bool isLoad = false;
    private int maxReachedLineIndex = 0;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();
    private LinkedList<string> historyRecords = new LinkedList<string>();
    public static VNManager Instance { get; private set; }

    #endregion
    #region Lifecycle
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
    void Start()
    {
        InitializeSaveFilePath();
        bottomButtonAddListener();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MenuManager.Instance.menuPanel.activeSelf
            && !SaveLoadManager.Instance.saveLoadPanel.activeSelf
            && !HistoryManager.Instance.historyScrollView.activeSelf
            && !SettingManager.Instance.settingPanel.activeSelf
            && gamePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!dialogueBox.activeSelf)
                {
                    OpenUI();
                }
                else if (!IsHittingBottomButtons())
                {
                    DisplayNextLine();
                }
            }
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (dialogueBox.activeSelf)
                {
                    CloseUI();
                }
                else
                {
                    OpenUI();
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                Debug.Log("°´ÏÂCtrl");
                CtrlSkip();
            }
        }
    }
    #endregion
    #region Initialization

    void InitializeSaveFilePath()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH);
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }
    void bottomButtonAddListener()
    {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        historyButton.onClick.AddListener(OnHistoryButtonClick);
        settingsButton.onClick.AddListener(OnSettingButtonClick);
        homeButton.onClick.AddListener(OnHomeButtoClick);
        closeButton.onClick.AddListener(OnCloseButtoClick);
    }

    public void StartGame()
    {
        InitializeAndLoadStory(defaultStoryFileName, defaultStartLine);
    }

    void InitializeAndLoadStory(string fileName, int lineNumber)
    {
        Initialize(lineNumber);
        LoadStoryFromFile(fileName);
        if (isLoad)
        {
            RecoverLastBackgroundAndAction();
            isLoad = false;
        }
        DisplayNextLine();
    }
    void Initialize(int line)
    {
        currentLine = line;

        backgroundImage.gameObject.SetActive(false);
        backgroundMusic.gameObject.SetActive(false);

        avatarImage.gameObject.SetActive(false);
        vocalAudio.gameObject.SetActive(false);

        characterImage1.gameObject.SetActive(false);
        characterImage2.gameObject.SetActive(false);

        choicePanel.SetActive(false);
    }

    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        var path = storyPath + currentStoryFileName + excelFileExtension;
        storyData = ExcelReader.ReadExcel(path);
        if (storyData == null || storyData.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        if (globalMaxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = globalMaxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion
    #region Display
    void DisplayNextLine()
    {
        if (currentLine > maxReachedLineIndex)
        {
            maxReachedLineIndex = currentLine;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if (currentLine >= storyData.Count - 1)
        {
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF, autoButton);
            }
            if (storyData[currentLine].speakerName == Constants.END_OF_STORY)
            {
                Debug.Log(Constants.END_OF_STORY);
            }
            if (storyData[currentLine].speakerName == Constants.CHOICE)
            {
                ShowChoices();
            }
            return;
        }
        if (typewritterEffect.IsTyping())
        {
            typewritterEffect.CompleteLine();
        }
        else
        {
            DisplayThisLine();
        }
    }
    void RecordHistory(string speaker, string content)
    {
        string historyRecord = speaker + Constants.COLON + content;
        if (historyRecords.Count >= Constants.MAX_LENGTH)
        {
            historyRecords.RemoveFirst();
        }
        historyRecords.AddLast(historyRecord);
    }

    void RecoverLastBackgroundAndAction()
    {
        var data = storyData[currentLine];
        if (NotNullNorEmpty(data.lastBackgroundImage))
        {
            UpdateBackgroundImage(data.lastBackgroundImage);
        }
        if (NotNullNorEmpty(data.lastBackgroundMusic))
        {
            PlayBackgroundAudio(data.lastBackgroundMusic);
        }
        if (data.character1Action != Constants.APPEAR_AT
            && NotNullNorEmpty(data.character1ImageFileName))
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character1ImageFileName,
                characterImage1, data.lastcoordinateX1);
        }
        if (data.character2Action != Constants.APPEAR_AT
            && NotNullNorEmpty(data.character2ImageFileName))
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character2ImageFileName,
                characterImage2, data.lastcoordinateX2);
        }
    }

    void DisplayThisLine()
    {
        var data = storyData[currentLine];
        speakerName.text = data.speakerName;
        currentSpeakingContent = data.speakingContent;
        typewritterEffect.StartTyping(currentSpeakingContent, currentTypingSpeed);

        RecordHistory(speakerName.text, currentSpeakingContent);

        if (NotNullNorEmpty(data.avatarImageFileName))
        {
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }
        if (NotNullNorEmpty(data.vocalAudioFileName))
        {
            PlayVocalAudio(data.vocalAudioFileName);
        }
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            PlayBackgroundAudio(data.backgroundMusicFileName);
        }
        if (NotNullNorEmpty(data.character1Action))
        {
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, data.coordinateX1);
        }
        if (NotNullNorEmpty(data.character2Action))
        {
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, data.coordinateX2);
        }

        currentLine++;
    }

    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }
    #endregion
    #region Choices
    void ShowChoices()
    {
        var data = storyData[currentLine];
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        choicePanel.SetActive(true);
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.speakingContent;
        choiceButton1.onClick.AddListener(() => InitializeAndLoadStory(data.avatarImageFileName, defaultStartLine));
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(() => InitializeAndLoadStory(data.backgroundImageFileName, defaultStartLine));
    }
    #endregion
    #region Audios
    void PlayAudio(string audioPath, AudioSource audioSource, bool isLoop)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            audioSource.loop = isLoop;
        }
        else
        {
            if (audioSource == vocalAudio)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FALED + audioPath);
            }
            else if (audioSource == backgroundMusic)
            {
                Debug.LogError(Constants.MUSIC_LOAD_FALED + audioPath);
            }
        }
    }

    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;
        PlayAudio(audioPath, vocalAudio, false);
    }
    void PlayBackgroundAudio(string musicFileName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFileName;
        PlayAudio(musicPath, backgroundMusic, true);
    }
    #endregion
    #region Image
    void UpdateAvatarImage(string imageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avatarImage);
    }

    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);
    }

    void UpdateCharacterImage(string action, string imageFileName, Image characterImage, string x)
    {
        if (action.StartsWith(Constants.APPEAR_AT))
        {
            string imagePath = Constants.CHARACTER_PATH + imageFileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, characterImage);
                var newPosition = new Vector2(float.Parse(x), characterImage.rectTransform.anchoredPosition.y);
                characterImage.rectTransform.anchoredPosition = newPosition;
                characterImage.DOFade(1, (isLoad ? 0 : Constants.DURATION_TIME)).From(0);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }

        }
        else if (action == Constants.DISAPPEAR)
        {
            characterImage.DOFade(0, Constants.DURATION_TIME).OnComplete(() => characterImage.gameObject.SetActive(false));
        }
        else if (action.StartsWith(Constants.MOVE_TO))
        {
            if (NotNullNorEmpty(x))
            {
                characterImage.rectTransform.DOAnchorPosX(float.Parse(x), Constants.DURATION_TIME);
            }
        }
    }

    void UpdateButtonImage(string imageFileName, Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath, button.image);
    }

    void UpdateImage(string imagePath, Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FALED + imagePath);
        }
    }
    #endregion
    #region Buttons
    #region Bottom
    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            Camera.main);
    }
    #endregion
    #region Auto
    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoButton);
        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }
    private IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typewritterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITTING_SECONDS);
        }
    }
    #endregion
    #region Skip
    void OnSkipButtonClick()
    {
        if (!isSkip && CanSkip())
        {
            StartSkip();
        }
        else if (isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }
    bool CanSkip()
    {
        return currentLine < maxReachedLineIndex;
    }
    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }
    void EndSkip()
    {
        isSkip = false;
        currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
    }
    void CtrlSkip()
    {
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipWhilePressingCtrl());
    }
    private IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITTING_SECONDS);
        }
    }
    private IEnumerator SkipWhilePressingCtrl()
    {
        while (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            DisplayNextLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITTING_SECONDS);
        }
    }
    #endregion
    #region Save
    void OnSaveButtonClick()
    {
        CloseUI();
        Texture2D screenshot = screenShotter.CaptureScreenshot();
        screenshotData = screenshot.EncodeToPNG();
        SaveLoadManager.Instance.ShowSavePanel(SaveGame);
        OpenUI();
    }
    void SaveGame(int slotIndex)
    {
        var saveData = new SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedSpeakingContent = currentSpeakingContent,
            savedScreenshotData = screenshotData,
            savedHistoryRecords = historyRecords
        };
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }

    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public string savedSpeakingContent;
        public byte[] savedScreenshotData;
        public LinkedList<string> savedHistoryRecords;
    }
    #endregion
    #region Load
    void OnLoadButtonClick()
    {
        ShowLoadPanel(null);
    }

    public void ShowLoadPanel(Action action)
    {
        SaveLoadManager.Instance.ShowLoadPanel(LoadGame, action);
    }
    void LoadGame(int slotIndex)
    {
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        if (File.Exists(savePath))
        {
            isLoad = true;
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            historyRecords = saveData.savedHistoryRecords;
            historyRecords.RemoveLast();
            var lineNumber = saveData.savedLine - 1;
            InitializeAndLoadStory(saveData.savedStoryFileName, lineNumber);
        }
    }
    #endregion
    #region Home
    void OnHomeButtoClick()
    {
        gamePanel.SetActive(false);
        MenuManager.Instance.menuPanel.SetActive(true);
    }
    #endregion
    #region Close
    void OnCloseButtoClick()
    {
        CloseUI();
    }

    void OpenUI()
    {
        dialogueBox.SetActive(true);
        bottomButtons.SetActive(true);
    }

    void CloseUI()
    {
        dialogueBox.SetActive(false);
        bottomButtons.SetActive(false);
    }
    #endregion
    #region History
    void OnHistoryButtonClick()
    {
        HistoryManager.Instance.ShowHistory(historyRecords);
    }
    #endregion
    #region Setting
    void OnSettingButtonClick()
    {
        SettingManager.Instance.ShowSettingPanel();
    }
    #endregion
    #endregion
}
