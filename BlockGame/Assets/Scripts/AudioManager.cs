using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    public Music[] musics;
    [Header("SFX")]
    public Sound[] sounds;

    private bool _closeSounds;
    public bool closeSounds
    {
        get => _closeSounds;
        set { if (_closeSounds != value)
            {
                _closeSounds = value;
                closeSound?.Invoke(_closeSounds);
            }

        }
    }
    public event Action<bool> closeSound;

    private bool _closeMusics;
    public bool closeMusics
    {
        get => _closeMusics;
        set
        {
            if (_closeMusics != value)
            {
                _closeMusics = value;
                closeMusic?.Invoke(_closeMusics);
            }

        }
    }
    public event Action<bool> closeMusic;
    public static AudioManager Instance { get; private set; }

    private void Start()
    {
        closeSound += SoundsClosed;
        closeMusic += MusicsClosed;
    }

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
        foreach (Music s in musics)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.volume = s.volume;
            s.source.clip = s.clip;
            s.source.playOnAwake = false;
            s.source.loop = s.loop;
        }
        _closeMusics = PlayerPrefs.GetInt("MusicClosed",1)==0;
        _closeSounds = PlayerPrefs.GetInt("SoundClosed",1)==0;
    }
    public void PlayMusic(string name)
    {
        if (closeMusics) return;
        Music m = Array.Find(musics, x => x.name == name);
        if (m != null)
        {
            m.source.Play();    
        }
        else { return; }
    }
    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }
    void SoundsClosed(bool b)
    {
        if (b)
        {
            foreach(Sound s in sounds)
            {
                s.source.volume = 0;
            }
        }
        else
        {
            foreach (Sound s in sounds)
            {
                s.source.volume = s.volume;
            }
        }
    }
    void MusicsClosed(bool b)
    {
        if (b)
        {
            foreach(Music m in musics)
            {
                m.source.volume = 0;
            }
        }
        else
        {
            foreach(Music s in musics)
            {
                s.source.volume = s.volume;
            }
        }
    }
    public void StopAllMusic()
    {
        foreach(Music s in musics)
        {
            s.source.Stop();
        }  
    }
    public void PlaySFX(string name)
    {
        if(closeSounds) return; 
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
[System.Serializable]   
public class Music
{
    public string name;
    public float volume;
    public AudioClip clip;
    [HideInInspector] public AudioSource source;
    public bool loop;
}
