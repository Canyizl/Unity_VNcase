using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button galleryButton;
    public Button settingsButton;
    public Button quitButton;

    private bool hasStarted = false;

    public static MenuManager Instance { get; private set; }

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
    // Start is called before the first frame update
    void Start()
    {
        menuButtonsAddListener();
    }

    void menuButtonsAddListener()
    {
        //startButton.onClick.AddListener(StartGame);
        startButton.onClick.AddListener(ShowInputPanel);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
        galleryButton.onClick.AddListener(ShowGalleryPanel);
        settingsButton.onClick.AddListener(ShowSettingPanel);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void StartGame()
    {
        hasStarted = true;
        VNManager.Instance.StartGame(Constants.DEFAULT_STORY_FILE_NAME, Constants.DEFAULT_START_LINE);
        ShowGamePanel();
    }

    private void ShowInputPanel()
    {
        InputManager.Instance.ShowInputPanel();
    }

    private void ContinueGame()
    {
        if (hasStarted)
        {
            ShowGamePanel();
        }
    }

    private void LoadGame()
    {
        VNManager.Instance.ShowLoadPanel(ShowGamePanel);
    }

    private void ShowGamePanel()
    {
        menuPanel.SetActive(false);
        VNManager.Instance.gamePanel.SetActive(true);
    }

    private void ShowGalleryPanel()
    {
        GalleryManager.Instance.ShowGalleryPanel();
    }

    private void ShowSettingPanel()
    {
        SettingManager.Instance.ShowSettingPanel();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
