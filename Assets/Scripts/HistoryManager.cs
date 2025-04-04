using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    public Transform historyContent;
    public GameObject historyItemPrefab;
    public GameObject historyScrollView;
    public Button closeButton;

    private LinkedList<ExcelReader.ExcelData> historyRecords;

    public static HistoryManager Instance { get; private set; }

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
        closeButton.onClick.AddListener(CloseHistory);
        closeButton.GetComponentInChildren<TextMeshProUGUI>().text = GetLocalized(Constants.CLOSE);
        ShowHistory(GameManager.Instance.historyRecords);
    }

    public void ShowHistory(LinkedList<ExcelReader.ExcelData> records)
    {
        foreach(Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<ExcelReader.ExcelData> currentNode = historyRecords.Last;
        while(currentNode != null)
        {
            var name = LM.GetSpeakerName(currentNode.Value);
            var content = LM.GetSpeakingContent(currentNode.Value);
            AddHistoryItem(name + GetLocalized(Constants.COLON) + content);
            currentNode = currentNode.Previous;
        }
        historyContent.GetComponent<RectTransform>().localPosition = Vector3.zero;
        historyScrollView.SetActive(true);
        historyScrollView.GetComponentInChildren<Scrollbar>().value = -3;
    }

    public void CloseHistory()
    {
        GameManager.Instance.historyRecords.RemoveLast();
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }

    private void AddHistoryItem(string text)
    {
        GameObject historyItem = Instantiate(historyItemPrefab, historyContent);
        historyItem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        historyItem.transform.SetAsFirstSibling();
    }

    string GetLocalized(string key)
    {
        return LocalizationManager.Instance.GetLocalizedValue(key);
    }
}
