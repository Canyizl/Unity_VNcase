using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VNManager : MonoBehaviour
{
    #region Variables
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerName;
    public TypewritterEffect typewritterEffect;
    public ScreenShotter screenShotter;

    public Image avatarImage;
    public Image backgroundImage;
    public Image characterImage1;
    public Image characterImage2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingsButton;
    public Button homeButton;
    public Button closeButton;

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
    private int maxReachedLineIndex = 0;

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
        GameManager.Instance.currentScene = Constants.GAME_SCENE;
        currentLine = GameManager.Instance.currentLineIndex;
        InitializeSaveFilePath();
        bottomButtonAddListener();
        InitializeImage();
        LoadStory(GameManager.Instance.currentStoryFile);
        DisplayNextLine();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogueBox.activeSelf)
            {
                OpenUI();
            }
            else if (!IsHittingBottomButtons() && !ChoiceManager.Instance.choicePanel.activeSelf)
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
            CtrlSkip();
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

    void LoadStory(string fileName)
    {
        LoadStoryFromFile(fileName);
        RecoverLastBackgroundAndCharacter();
    }

    void InitializeImage()
    {
        backgroundImage.gameObject.SetActive(false);
        avatarImage.gameObject.SetActive(false);
        characterImage1.gameObject.SetActive(false);
        characterImage2.gameObject.SetActive(false);
    }

    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        var filePath = Path.Combine(Application.streamingAssetsPath,
                        Constants.STORY_PATH,
                        fileName + Constants.STORY_FILE_EXTENSION
            );
        storyData = ExcelReader.ReadExcel(filePath);
        if (storyData == null || storyData.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        GameManager.Instance.currentStoryFile = currentStoryFileName;

        if (GameManager.Instance.maxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = GameManager.Instance.maxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion
    #region Display
    void DisplayNextLine()
    {
        if (currentLine > maxReachedLineIndex)
        {
            maxReachedLineIndex = currentLine;
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
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
                GameManager.Instance.hasStarted = false;
                SceneManager.LoadScene(Constants.CREDITS_SCENE);
            }
            if (storyData[currentLine].speakerName == Constants.CHOICE)
            {
                ShowChoices();
            }
            if (storyData[currentLine].speakerName == Constants.GOTO)
            {
                LoadStory(storyData[currentLine].speakingContent);
                currentLine = Constants.DEFAULT_START_LINE;
                DisplayNextLine();
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

    void DisplayThisLine()
    {
        GameManager.Instance.currentLineIndex = currentLine;
        var data = storyData[currentLine];
        speakerName.text = LM.GetSpeakerName(data);
        currentSpeakingContent = LM.GetSpeakingContent(data);
        typewritterEffect.StartTyping(currentSpeakingContent, currentTypingSpeed);

        GameManager.Instance.historyRecords.AddLast(data);

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
            GameManager.Instance.currentBackgroundImg = data.backgroundImageFileName;
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            GameManager.Instance.currentBackgroundMusic = data.backgroundMusicFileName;
            PlayBackgroundAudio(data.backgroundMusicFileName);
        }
        if (NotNullNorEmpty(data.character1Action))
        {
            if (data.character1Action == Constants.DISAPPEAR)
            {
                GameManager.Instance.isCharacter1Display = false;
            }
            else
            {
                GameManager.Instance.isCharacter1Display = true;
                GameManager.Instance.currentCharacter1Img = data.character1ImageFileName;
                GameManager.Instance.currentCharacter1Position = data.coordinateX1;
            }
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, data.coordinateX1);
        }
        if (NotNullNorEmpty(data.character2Action))
        {
            if (data.character2Action == Constants.DISAPPEAR)
            {
                GameManager.Instance.isCharacter2Display = false;
            }
            else
            {
                GameManager.Instance.isCharacter2Display = true;
                GameManager.Instance.currentCharacter2Img = data.character2ImageFileName;
                GameManager.Instance.currentCharacter2Position = data.coordinateX2;
            }
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, data.coordinateX2);
        }

        currentLine++;
    }

    void RecoverLastBackgroundAndCharacter()
    {
        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundImg))
        {
            UpdateBackgroundImage(GameManager.Instance.currentBackgroundImg);
        }
        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundMusic))
        {
            PlayBackgroundAudio(GameManager.Instance.currentBackgroundMusic);
        }
        if (GameManager.Instance.isCharacter1Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT_INSTANTLY, GameManager.Instance.currentCharacter1Img,
                                characterImage1, GameManager.Instance.currentCharacter1Position);
        }
        if (GameManager.Instance.isCharacter2Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT_INSTANTLY, GameManager.Instance.currentCharacter2Img,
                                characterImage2, GameManager.Instance.currentCharacter2Position);
        }
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
        var choices = LM.GetSpeakingContent(data).Split(Constants.ChoiceDelimiter).Select(s => s.Trim()).ToList();
        var actions = data.avatarImageFileName.Split(Constants.ChoiceDelimiter).Select(s => s.Trim()).ToList();
        ChoiceManager.Instance.ShowChoices(choices, actions, HandleChoice);
    }

    void HandleChoice(string selectedChoice)
    {
        currentLine = Constants.DEFAULT_START_LINE;
        LoadStory(selectedChoice);
        DisplayNextLine();
    }
    #endregion
    #region Audios
    void PlayVocalAudio(string audioFileName)
    {
        AudioManager.Instance.PlayVoice(audioFileName);
    }
    void PlayBackgroundAudio(string musicFileName)
    {
        AudioManager.Instance.PlayBackground(musicFileName);
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
        if (!GameManager.Instance.unlockedBackgrounds.Contains(imageFileName))
        {
            GameManager.Instance.unlockedBackgrounds.Add(imageFileName);
        }
    }

    void UpdateCharacterImage(string action, string imageFileName, Image characterImage, string x)
    {
        // 根据action执行对应的动画或操作
        if (action.StartsWith(Constants.APPEAR_AT))
        {
            string imagePath = Constants.CHARACTER_PATH + imageFileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, characterImage);
                var newPosition = new Vector2(float.Parse(x), characterImage.rectTransform.anchoredPosition.y);
                characterImage.rectTransform.anchoredPosition = newPosition;

                var duration = Constants.DURATION_TIME;
                if (action == Constants.APPEAR_AT_INSTANTLY)
                {
                    duration = 0;
                }
                characterImage.DOFade(1, duration).From(0);
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
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
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
        return true;
        //return currentLine < maxReachedLineIndex;
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
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
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
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
    }
    #endregion
    #region Save
    void OnSaveButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Save;
        SceneManager.LoadScene(Constants.SAVE_LOAD_SCENE);
    }
    #endregion
    #region Load
    void OnLoadButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_LOAD_SCENE);
    }
    #endregion
    #region Home
    void OnHomeButtoClick()
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
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
        SceneManager.LoadScene(Constants.HISTORY_SCENE);
    }
    #endregion
    #region Setting
    void OnSettingButtonClick()
    {
        SceneManager.LoadScene(Constants.SETTING_SCENE);
    }
    #endregion
    #endregion
}
