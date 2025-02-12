using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VNManager : MonoBehaviour
{
    public GameObject gamePanel;
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerName;
    public TextMeshProUGUI speakingContent;
    public TypewritterEffect typewritterEffect;
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
    private string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;
    private List<ExcelReader.ExcelData> storyData;
    private int currentLine = Constants.DEFAULT_START_LINE;
    private string currentStoryFileName;

    private bool isAutoPlay = false;
    private bool isSkip = false;
    private int maxReachedLineIndex = 0;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();
    // Start is called before the first frame update

    public static VNManager Instance { get; private set; }
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
        bottomButtonAddListener();
        gamePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePanel.activeSelf && Input.GetMouseButtonDown(0))
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
    }

    void bottomButtonAddListener()
    {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);

        homeButton.onClick.AddListener(OnHomeButtoClick);
        closeButton.onClick.AddListener(OnCloseButtoClick);
    }

    public void StartGame()
    {
        InitializeAndLoadStory(defaultStoryFileName);
    }

    void InitializeAndLoadStory(string fileName)
    {
        Initialize();
        LoadStoryFromFile(fileName);
        DisplayNextLine();
    }
    void Initialize()
    {
        currentLine = Constants.DEFAULT_START_LINE;
        avatarImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        characterImage1.gameObject.SetActive(false);
        characterImage2.gameObject.SetActive(false);
        choicePanel.SetActive(false);
    }
    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        var path = storyPath + fileName + excelFileExtension;
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

    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            null);
    }

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

    void DisplayThisLine()
    {
        var data = storyData[currentLine];
        speakerName.text = data.speakerName;
        typewritterEffect.StartTyping(data.speakingContent);
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

    void ShowChoices()
    {
        var data = storyData[currentLine];
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        choicePanel.SetActive(true);
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.speakingContent;
        choiceButton1.onClick.AddListener(() => InitializeAndLoadStory(data.avatarImageFileName));
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(() => InitializeAndLoadStory(data.backgroundImageFileName));
    }

    void UpdateAvatarImage(string imageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avatarImage);
    }

    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;
        PlayAudio(audioPath, vocalAudio, false);
    }

    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);
    }

    void PlayBackgroundAudio(string musicFileName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFileName;
        PlayAudio(musicPath, backgroundMusic, true);
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
                characterImage.DOFade(1, Constants.DURATION_TIME).From(0);
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

    void PlayAudio(string audioPath, AudioSource audioSource, bool isLoop)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            backgroundMusic.clip = audioClip;
            backgroundMusic.Play();
            backgroundMusic.loop = isLoop;
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


    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoButton);
        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }

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

    void OnSaveButtonClick()
    {
        SaveLoadManager.Instance.ShowSaveLoadUI(true);
    }

    void OnLoadButtonClick()
    {
        SaveLoadManager.Instance.ShowSaveLoadUI(false);
    }

    bool CanSkip()
    {
        return currentLine < maxReachedLineIndex;
    }

    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        typewritterEffect.typingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }

    void EndSkip()
    {
        isSkip = false;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
        typewritterEffect.typingSpeed = Constants.DEFAULT_TYPING_SPEED;
    }

    void UpdateButtonImage(string imageFileName, Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath, button.image);
    }

    private IEnumerator StartAutoPlay()
    {
        while(isAutoPlay)
        {
            if (!typewritterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITTING_SECONDS);
        }
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

    void OnHomeButtoClick()
    {
        gamePanel.SetActive(false);
        MenuManager.Instance.menuPanel.SetActive(true);
    }

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
}
