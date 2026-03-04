using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : Persistence<AudioManager>
{
    [Header("References")]
    [Tooltip("Optional: assign a SoundLibrary ScriptableObject. If null, will try to load from Resources/SoundLibrary")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // scene-level override clips (designers can drag clip here to override library defaults)
    [Header("Scene Overrides (optional)")]
    [SerializeField] private List<Sound> sceneMusics = new List<Sound>();
    [SerializeField] private List<Sound> sceneSfxs = new List<Sound>();

    public AudioSource MusicSource { get => musicSource; set => musicSource = value; }
    public AudioSource SfxSource { get => sfxSource; set => sfxSource = value; }

    protected override void Awake()
    {
        base.Awake();

        if (soundLibrary == null)
        {
            soundLibrary = Resources.Load<SoundLibrary>("SoundLibrary");
            if (soundLibrary == null)
            {
                Debug.LogWarning("AudioManager: No SoundLibrary assigned and none found in Resources/SoundLibrary. Designer should create one.");
            }
        }

        if (musicSource == null || sfxSource == null)
        {
            // Try to find or create audio sources on the same GameObject
            var sources = gameObject.GetComponentsInChildren<AudioSource>();
            if (sources != null)
            {
                if (musicSource == null && sources.Length > 0) musicSource = sources[0];
                if (sfxSource == null && sources.Length > 1) sfxSource = sources.Length > 1 ? sources[1] : sources[0];
            }
        }
    }

    private void Start()
    {
        // Auto-play startup music from library if configured
        string startup = soundLibrary != null ? soundLibrary.startupMusicName : null;
        if (!string.IsNullOrEmpty(startup))
        {
            PlayMusic(startup);
        }
    }

    // Play music by name. Search scene overrides first, then library.
    public void PlayMusic(string name)
    {
        Sound s = sceneMusics.Find(x => x.name == name);
        if (s == null && soundLibrary != null)
            s = soundLibrary.musics.Find(x => x.name == name);

        if (s != null && s.clip != null && musicSource != null)
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"PlayMusic: music '{name}' not found or no AudioSource assigned.");
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    // Stop currently playing music
    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;
    }

    // Play SFX by registered name (search scene overrides then library)
    public void PlaySFX(string name)
    {
        Sound s = sceneSfxs.Find(x => x.name == name);
        if (s == null && soundLibrary != null)
            s = soundLibrary.sfxs.Find(x => x.name == name);

        if (s != null && s.clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(s.clip);
        }
        else
        {
            Debug.LogWarning($"PlaySFX: sfx '{name}' not found or no AudioSource assigned.");
        }
    }

    // Play SFX from a direct AudioClip
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public bool ToggleMusic()
    {
        if (musicSource == null) return false;
        musicSource.mute = !musicSource.mute;
        return musicSource.mute;
    }

    public bool ToggleSFX()
    {
        if (sfxSource == null) return false;
        sfxSource.mute = !sfxSource.mute;
        return sfxSource.mute;
    }

    public void AdjustMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
    }

    public void AdjustSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
    }
}
