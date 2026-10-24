using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;


    public List<Sound> sounds = new List<Sound>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        
    }

    private void Start()
    {
        
    }


    public void SetAudio()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.audioSource == null)
            {
                sound.audioSource = gameObject.AddComponent<AudioSource>();
                sound.audioSource.clip = sound.clip;
                sound.audioSource.volume = sound.volume;
                sound.audioSource.loop = sound.loop;
            }
        }
    }

    public static SoundManager GetInstance()
    {
        return instance;
    }

    public void Play(string name)
    {
        Sound sound = sounds.Find(s => s.name == name);
        if (sound != null && sound.audioSource != null)
        {
            sound.audioSource.Play();
        }
    }

    public void Stop(string name)
    {
        Sound sound = sounds.Find(s => s.name == name);
        if (sound != null && sound.audioSource != null)
        {
            sound.audioSource.Stop();
        }
    }

    public void OnGamePaused(bool isPaused)
    {
            Sound GamePlaysound = sounds.Find(s => s.name == "GamePlay");
            Sound PauseSound = sounds.Find(s => s.name == "Pause");
        if (isPaused)
        {
            GamePlaysound.audioSource.Pause();
            PauseSound.audioSource.Play();
        }
        else
        {
            GamePlaysound.audioSource.UnPause();
            PauseSound.audioSource.Stop();
        }
    }

    public void Mute(bool audioOn)
    {
        foreach (Sound sound in sounds)
        {
            sound.audioSource.mute = audioOn;
        }
    }
    public void SetVolume(float volume)
    {
        foreach(Sound s in sounds)
        {
            s.volume = volume;
        }
    }
}
