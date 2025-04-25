using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader.Log;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public UnityEvent OnVideoSkipped;
    public bool isPlaying = false;
    private List<string> videoList = new List<string>();
    private static string lastPlayedVideo = "";
    private bool canSkip = false;
    public Image logoImage;
    private float fadeInDuration = 0.5f;
    private float displayDuration = 0.8f;

    public static IntroManager Instance { get; private set; }

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
        logoImage.color = new Color(1, 1, 1, 0);
        string videoPath = Path.Combine(Application.streamingAssetsPath, Constants.VIDEO_PATH);
        if (Directory.Exists(videoPath))
        {
            string[] videoFiles = Directory.GetFiles(videoPath, "*" + Constants.VIDEO_FILE_EXTENTION);
            foreach (string videoFile in videoFiles)
            {
                videoList.Add(videoFile);
            }
        }
        StartCoroutine(FadeLogo());
    }

    private void Update()
    {
        if (canSkip && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            SkipVideo();
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            logoImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    IEnumerator FadeLogo()
    {
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Fade(0, 1, fadeInDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(Fade(1, 0, fadeInDuration));
        if (logoImage.color == new Color(1, 1, 1, 0))
        {
           PlayRandomVideo();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    void PlayRandomVideo()
    {
        if (videoList.Count > 0)
        {
            FadeInEffect.Instance.overlayFadeIn();
            isPlaying = true;
            int randomIndex = Random.Range(0, videoList.Count);
            lastPlayedVideo = videoList[randomIndex];
            videoPlayer.url = lastPlayedVideo;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
            StartCoroutine(EnableSkipAfterDelay(0.3f));
        }
        else
        {
            SceneManager.LoadScene(Constants.MENU_SCENE);
        }
    }
    IEnumerator EnableSkipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canSkip = true;
    }

    void SkipVideo()
    {
        if (!canSkip) return;
        videoPlayer.loopPointReached -= OnVideoEnd;
        FadeInEffect.Instance.OnVideoSkipped();
        videoPlayer.Stop();
        isPlaying = false;
        canSkip = false;
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    public static string GetLastPlayedVideo()
    {
        return lastPlayedVideo;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
