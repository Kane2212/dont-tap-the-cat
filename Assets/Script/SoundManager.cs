using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public List<Sound> sounds = new List<Sound>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SetAudio();
    }

    public void SetAudio()
    {
        foreach (Sound sound in sounds)
        {
            if (sound == null)
            {
                continue;
            }

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
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        Sound sound = sounds.Find(s => s != null && s.name == name);
        if (sound == null || sound.audioSource == null)
        {
            return;
        }

        sound.audioSource.Play();
    }

    public void Stop(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        Sound sound = sounds.Find(s => s != null && s.name == name);
        if (sound != null && sound.audioSource != null)
        {
            sound.audioSource.Stop();
        }
    }

    public void OnGamePaused(bool isPaused)
    {
        Sound gamePlaySound = sounds.Find(s => s != null && s.name == "GamePlay");
        Sound pauseSound = sounds.Find(s => s != null && s.name == "Pause");

        if (isPaused)
        {
            if (gamePlaySound != null && gamePlaySound.audioSource != null)
            {
                gamePlaySound.audioSource.Pause();
            }

            if (pauseSound != null && pauseSound.audioSource != null)
            {
                pauseSound.audioSource.Play();
            }
        }
        else
        {
            if (gamePlaySound != null && gamePlaySound.audioSource != null)
            {
                gamePlaySound.audioSource.UnPause();
            }

            if (pauseSound != null && pauseSound.audioSource != null)
            {
                pauseSound.audioSource.Stop();
            }
        }
    }

    public void Mute(bool audioOn)
    {
        foreach (Sound sound in sounds)
        {
            if (sound != null && sound.audioSource != null)
            {
                sound.audioSource.mute = !audioOn;
            }
        }
    }

    public void SetVolume(float volume)
    {
        foreach (Sound s in sounds)
        {
            if (s == null)
            {
                continue;
            }

            s.volume = volume;
            if (s.audioSource != null)
            {
                s.audioSource.volume = volume;
            }
        }
    }
}
