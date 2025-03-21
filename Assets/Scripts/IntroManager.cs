using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private List<string> videoList = new List<string>();
    private static string lastPlayedVideo = "";
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

    // Update is called once per frame
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    void PlayRandomVideo()
    {
        if (videoList.Count > 0)
        {
            int randomIndex = Random.Range(0, videoList.Count);
            lastPlayedVideo = videoList[randomIndex];
            videoPlayer.url = lastPlayedVideo;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            SceneManager.LoadScene(Constants.MENU_SCENE);
        }
    }

    public static string GetLastPlayedVideo()
    {
        return lastPlayedVideo;
    }
}
