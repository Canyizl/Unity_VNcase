using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class SaveLoadManager : MonoBehaviour
{
    public GameObject saveLoadPanel;
    public TextMeshProUGUI panelTitle;
    public Button[] saveLoadButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;

    public bool isSave;
    public int currentPage = Constants.DEFAULT_START_INDEX;
    public readonly int slotsPerpage = Constants.SLOTS_PER_PAGE;
    public readonly int totalSlots = Constants.TOTAL_SLOTS;
    private System.Action<int> currentAction;
    private System.Action menuAction;

    public static SaveLoadManager Instance { get; private set; }

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
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);
        saveLoadPanel.SetActive(false);
    }

    public void ShowSavePanel(System.Action<int> action)
    {
        isSave = true;
        ShowPanel(action);
    }
    public void ShowLoadPanel(System.Action<int> action, System.Action menuAction)
    {
        isSave = false;
        ShowPanel(action);
    }

    private void ShowPanel(System.Action<int> action)
    {
        panelTitle.text = LocalizationManager.Instance.GetLocalizedValue(isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME);
        prevPageButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.PREV_PAGE);
        nextPageButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.NEXT_PAGE);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.BACK);
        currentAction = action;
        UpdateUI();
        saveLoadPanel.SetActive(true);
    }
    private void UpdateUI()
    {
        for (int i = 0; i < slotsPerpage; i++)
        {
            int slotIndex = currentPage * slotsPerpage + i;
            if (slotIndex < totalSlots)
            {
                UpdateSaveLoadButtons(saveLoadButtons[i], slotIndex);
                LoadStorylineAndScreenshots(saveLoadButtons[i], slotIndex);
            }
            else
            {
                saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void UpdateSaveLoadButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        var savePath = GenerateDataPath(index);
        var fileExists = File.Exists(savePath);

        if (!isSave && !fileExists)
        {
            button.interactable = false;
        }

        var textCompents = button.GetComponentsInChildren<TextMeshProUGUI>();
        textCompents[0].text = null;
        textCompents[1].text = (index + 1) + LocalizationManager.Instance.GetLocalizedValue(Constants.COLON) + LocalizationManager.Instance.GetLocalizedValue(Constants.EMPTY_SLOT);
        button.GetComponentInChildren<RawImage>().texture = null;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(button, index));

    }
    private void OnButtonClick(Button button, int index)
    {
        currentAction?.Invoke(index);
        menuAction?.Invoke();
        if (isSave)
        {
            LoadStorylineAndScreenshots(button, index);
        }
        else
        {
            GoBack();
        }
    }
    private string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerpage < totalSlots)
        {
            currentPage++;
            UpdateUI();
        }
    }

    private void GoBack()
    {
        saveLoadPanel.SetActive(false);
    }

    private void LoadStorylineAndScreenshots(Button button, int index)
    {
        var savePath = GenerateDataPath(index);
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<VNManager.SaveData>(json);
            if (saveData.savedScreenshotData != null)
            {
                Texture2D screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(saveData.savedScreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
            }
            if (saveData.savedSpeakingContent != null)
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = saveData.savedSpeakingContent;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }
        }
    }
}
