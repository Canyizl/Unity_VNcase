using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
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
promptText.text = LocalizationManager.Instance.GetLocalizedValue(Constants.PROMPT_TEXT);
        nameInputField.text = "";
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.CONFIRM);
        confirmButton.onClick.AddListener(OnConfirm);

    }

    void OnConfirm()
    {
        string playerName = nameInputField.text.Trim();
        if (IsInvalidName(playerName))
        {
            return;
        }
        GameManager.Instance.playerName = playerName;
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }

    bool IsInvalidName(string name)
    {
        return string.IsNullOrEmpty(name);
    }
}
