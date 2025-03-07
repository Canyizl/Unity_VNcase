using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public GameObject inputPanel;
    public TextMeshProUGUI promptText;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public static InputManager Instance { get; private set; }

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
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.CONFIRM;
        confirmButton.onClick.AddListener(OnConfirm);
        inputPanel.SetActive(false);

    }

    void OnConfirm()
    {
        string playerName = nameInputField.text.Trim();
        if (IsInvalidName(playerName))
        {
            return;
        }
        PlayerData.Instance.playerName = playerName;
        inputPanel.SetActive(false);
        MenuManager.Instance.StartGame();
    }

    bool IsInvalidName(string name)
    {
        return string.IsNullOrEmpty(name);
    }

    public void ShowInputPanel()
    {
        promptText.text = Constants.PROMPT_TEXT;
        nameInputField.text = "";
        inputPanel.SetActive(true);
    }


}
