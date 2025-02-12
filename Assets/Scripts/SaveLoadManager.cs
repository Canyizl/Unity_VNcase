using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void ShowSaveLoadUI(bool save)
    {
        isSave = save;
        panelTitle.text = isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME;
        UpdateSaveLoadUI();
        saveLoadPanel.SetActive(true);
        LoadStorylineAndScreenshots();
    }

    private void UpdateSaveLoadUI()
    {
        for (int i = 0; i < slotsPerpage; i++)
        {
            int slotIndex = currentPage * slotsPerpage + i;
            if (slotIndex < totalSlots)
            {
                saveLoadButtons[i].gameObject.SetActive(true);
                saveLoadButtons[i].interactable = true;

                var slotText = (slotIndex + 1) + Constants.COLON + Constants.EMPTY_SLOT;
                var textCompents = saveLoadButtons[i].GetComponentsInChildren<TextMeshProUGUI>();
                textCompents[0].text = null;
                textCompents[1].text = slotText;
                saveLoadButtons[i].GetComponentInChildren<RawImage>().texture = null;
            }
            else
            {
                saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateSaveLoadUI();
            LoadStorylineAndScreenshots();
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerpage < totalSlots)
        {
            currentPage++;
            UpdateSaveLoadUI();
            LoadStorylineAndScreenshots();
        }
    }

    private void GoBack()
    {
        saveLoadPanel.SetActive(false);
    }

    private void LoadStorylineAndScreenshots()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
