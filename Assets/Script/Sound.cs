using UnityEngine;
public enum AudioType
{
    SoundEffect,
    Music
}
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume = 1f;
    public bool loop = false;
    public AudioType type;


    [HideInInspector] public AudioSource audioSource;
}
