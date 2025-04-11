using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Text toggleLabel;
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] avaiableResolutions; 
    private Resolution defaultResolution;
    public Button defaultButton;
    public Button closeButton;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider voiceVolumeSlider;
    public AudioMixer audioMixer;

    public static SettingManager Instance { get; private set; }

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
        AddListener();
        Initialization();
    }

    void Initialization()
    {
        InitializeDisplayMode();
        InitializeResolutions();
        InitializeButtons();
        InitializeVolume();
    }

    void InitializeDisplayMode()
    {
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        UpdateToggleLabel(fullscreenToggle.isOn);
    }

    void InitializeButtons()
    {
        closeButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.CLOSE);
        defaultButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue(Constants.RESET);
    }

    void InitializeVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat(Constants.MASTER_VOLUME, Constants.DEFAULT_VOLUME);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(Constants.MUSIC_VOLUME, Constants.DEFAULT_VOLUME);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat(Constants.VOICE_VOLUME, Constants.DEFAULT_VOLUME);
    }

    void AddListener()
    {
        fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        closeButton.onClick.AddListener(CloseSetting);
        defaultButton.onClick.AddListener(ResetSetting);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
    }

    void InitializeResolutions()
    {
        avaiableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        var resolutionMap = new Dictionary<string, Resolution>();
        int currentResolutionIndex = 0;

        foreach(var res in avaiableResolutions)
        {
            const float aspectRatio = 16f / 9f;
            const float epsilon = 0.01f;

            if (Mathf.Abs((float)res.width / res.height - aspectRatio) > epsilon)
                continue;

            string option = res.width + "x" + res.height;
            if (!resolutionMap.ContainsKey(option))
            {
                resolutionMap[option] = res;
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = resolutionDropdown.options.Count - 1;
                    defaultResolution = res;
                }
            }
            
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    void SetDisplayMode(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(isFullscreen);
    }

    void UpdateToggleLabel(bool isFullscreen)
    {
        toggleLabel.text = isFullscreen ? LocalizationManager.Instance.GetLocalizedValue(Constants.FULLSCREEN): LocalizationManager.Instance.GetLocalizedValue(Constants.WINDOWED); ;
    }

    void SetResolution(int index)
    {
        string[] dimensions = resolutionDropdown.options[index].text.Split('x');
        int width = int.Parse(dimensions[0].Trim());
        int height = int.Parse(dimensions[1].Trim());
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }

    private float SliderValueToDecibel(float value)
    {
        return value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
    }

    void SetMasterVolume(float value)
    {
        audioMixer.SetFloat(Constants.MASTER_VOLUME, SliderValueToDecibel(value));
    }

    void SetMusicVolume(float value)
    {
        audioMixer.SetFloat(Constants.MUSIC_VOLUME, SliderValueToDecibel(value));
    }

    void SetVoiceVolume(float value)
    {
        audioMixer.SetFloat(Constants.VOICE_VOLUME, SliderValueToDecibel(value));
    }

    void CloseSetting()
    {
        var sceneName = GameManager.Instance.currentScene;
        if (sceneName == Constants.GAME_SCENE)
        {
            GameManager.Instance.historyRecords.RemoveLast();
        }

        PlayerPrefs.SetFloat(Constants.MASTER_VOLUME, Constants.DEFAULT_VOLUME);
        PlayerPrefs.SetFloat(Constants.MUSIC_VOLUME, Constants.DEFAULT_VOLUME);
        PlayerPrefs.SetFloat(Constants.VOICE_VOLUME, Constants.DEFAULT_VOLUME);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }

    void ResetSetting()
    {
        resolutionDropdown.value = resolutionDropdown.options.FindIndex(
            option => option.text == $"{defaultResolution.width}x{defaultResolution.height}");
        fullscreenToggle.isOn = true;

        masterVolumeSlider.value = Constants.DEFAULT_VOLUME;
        musicVolumeSlider.value = Constants.DEFAULT_VOLUME;
        voiceVolumeSlider.value = Constants.DEFAULT_VOLUME;

        SetMasterVolume(masterVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetVoiceVolume(voiceVolumeSlider.value);
    }
}
