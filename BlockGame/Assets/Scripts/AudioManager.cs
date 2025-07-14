using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    public Sound[] sounds;

    public static AudioManager Instance { get; private set; }    
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;    
        }
        DontDestroyOnLoad(gameObject);
        foreach (Sound s in sounds) 
        {
            s.source=gameObject.AddComponent<AudioSource>();
            s.source.volume=s.volume;
            s.source.clip = s.clip;
            s.source.playOnAwake = false;
        }
    }
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sounds, x => x.name == name);
        if (s != null)
        {
            s.source.Play();
        }
        else
        {
            return;
        }
    }

}
[System.Serializable]
public class Sound
{
    public string name; 
    public float volume;
    public AudioClip clip;
    [HideInInspector]public AudioSource source;
}
