using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;

    private AudioSource mainAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainAudioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip(AudioClip clip)
    {
        mainAudioSource.clip = clip;
        mainAudioSource.Play();
    }

}
