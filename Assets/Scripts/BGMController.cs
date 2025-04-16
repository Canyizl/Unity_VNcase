using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class BGMController : MonoBehaviour
{
    public AudioClip bgmClip;
    public float volume = 0.6f;  
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("No audioSource found!");
        }

        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.Play();
    }
}