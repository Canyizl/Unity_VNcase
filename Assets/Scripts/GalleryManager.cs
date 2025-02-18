using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    public GameObject galleryPanel;
    public TextMeshProUGUI panelTitle;
    public Button[] galleryButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;
    public GameObject bigImagePanel;
    public Image bigImage;

    public int currentPage = Constants.DEFAULT_START_INDEX;
    public readonly int slotsPerpage = Constants.GALLERY_SLOTS_PER_PAGE;
    public readonly int totalSlots = Constants.ALL_BACKGROUNDS.Length;
    public static GalleryManager Instance { get; private set; }

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
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);
        galleryPanel.SetActive(false);
        panelTitle.text = Constants.GALLERY;

        bigImagePanel.SetActive(false);
        Button bigImageButton = bigImagePanel.GetComponent<Button>();
        if (bigImageButton != null)
        {
            bigImageButton.onClick.AddListener(HideBigImage);
        }
        else
        {
            Debug.LogWarning("BigImagePanelÉÏµÄButtonÄØ£¿");
        }
    }

    public void ShowGalleryPanel()
    {
        UpdateUI();
        galleryPanel.SetActive(true);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slotsPerpage; i++)
        {
            int slotIndex = currentPage * slotsPerpage + i;
            if (slotIndex < totalSlots)
            {
                UpdateGalleryButtons(galleryButtons[i], slotIndex);
            }
            else
            {
                galleryButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateGalleryButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        string bgName = Constants.ALL_BACKGROUNDS[index];
        bool isUnlocked = VNManager.Instance.unlockedBackgrounds.Contains(bgName);

        string imagePath = Constants.THUMBNAIL_PATH + (isUnlocked ? bgName : Constants.GALLERY_PLACEHOLDER);
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            button.GetComponentInChildren<Image>().sprite = sprite;
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FALED + imagePath);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(button, index));
    }

    private void OnButtonClick(Button button, int index)
    {
        string bgName = Constants.ALL_BACKGROUNDS[index];
        bool isUnlocked = VNManager.Instance.unlockedBackgrounds.Contains(bgName);

        if (!isUnlocked)
        {
            return;
        }

        string imagePath = Constants.BACKGROUND_PATH + bgName;
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            bigImage.sprite = sprite;
            bigImagePanel.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.BIG_IMAGE_LOAD_FALED + imagePath);
        }
    }

    private void HideBigImage()
    {
        bigImagePanel.SetActive(false);
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
        galleryPanel.SetActive(false);
    }
}
