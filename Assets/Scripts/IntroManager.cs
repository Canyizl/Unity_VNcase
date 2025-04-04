using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader.Log;
using System.Collections;
using UnityEngine.Events;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public UnityEvent OnVideoSkipped;
    public bool isPlaying = false;
    private List<string> videoList = new List<string>();
    private static string lastPlayedVideo = "";
    private bool canSkip = true;

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
        string videoPath = Path.Combine(Application.streamingAssetsPath, Constants.VIDEO_PATH);
        if (Directory.Exists(videoPath))
        {
            string[] videoFiles = Directory.GetFiles(videoPath, "*" + Constants.VIDEO_FILE_EXTENTION);
            foreach (string videoFile in videoFiles)
            {
                videoList.Add(videoFile);
            }
        }
        PlayRandomVideo();
    }

    private void Update()
    {
        if (canSkip && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            SkipVideo();
        }
    }

    // Update is called once per frame
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    void PlayRandomVideo()
    {
        if (videoList.Count > 0)
        {
            isPlaying = true;
            int randomIndex = Random.Range(0, videoList.Count);
            lastPlayedVideo = videoList[randomIndex];
            videoPlayer.url = lastPlayedVideo;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;

            EnableSkipAfterDelay(0.2f);
        }
        else
        {
            SceneManager.LoadScene(Constants.MENU_SCENE);
        }
    }
    IEnumerator EnableSkipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canSkip = true; // 1Ãëºó½âËøÌø¹ý
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
