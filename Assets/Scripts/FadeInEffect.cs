using System.Collections;
using UnityEngine;

public class FadeInEffect : MonoBehaviour
{
    public CanvasGroup blackOverlay;
    public float fadeDuration = 3.0f;

    public static FadeInEffect Instance { get; private set; }

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
        StartCoroutine(FadeOut());
    }

    public void OnVideoSkipped()
    { 
        blackOverlay.alpha = 1;
        StopAllCoroutines();
    }

    IEnumerator FadeOut()
    {
        float time = 0;
        while ( time < fadeDuration)
        {
            blackOverlay.alpha = 1 - (time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        blackOverlay.alpha = 0;
        blackOverlay.gameObject.SetActive(false);
    }

}
