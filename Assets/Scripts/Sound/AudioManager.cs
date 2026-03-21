using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("References")]
    [Tooltip("Assign SoundLibrary ScriptableObject. Nếu để trống, sẽ tự tìm trong Resources/SoundLibrary.")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    public AudioSource MusicSource => musicSource;
    public AudioSource SfxSource  => sfxSource;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        TryLoadLibrary();
        TryAutoAssignSources();
    }

    private void Start()
    {
        if (soundLibrary != null && soundLibrary.startupTrack != MusicTrack.None)
            PlayMusic(soundLibrary.startupTrack);
    }

    // ── Music ──────────────────────────────────────────────────────────────────

    /// <summary>Phát nhạc theo enum (cách được khuyến nghị).</summary>
    public void PlayMusic(MusicTrack track)
    {
        if (!ValidateLibraryAndSource(musicSource, "PlayMusic")) return;
        var clip = soundLibrary.GetMusic(track);
        if (clip != null) PlayMusicClip(clip);
    }

    /// <summary>Phát nhạc trực tiếp bằng AudioClip (dùng khi cần override cụ thể).</summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        PlayMusicClip(clip);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;
    }

    public bool ToggleMusic()
    {
        if (musicSource == null) return false;
        musicSource.mute = !musicSource.mute;
        return musicSource.mute;
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(volume);
    }

    // ── SFX ───────────────────────────────────────────────────────────────────

    /// 
    public void PlaySFX(SFXClip sfx)
    {
        if (!ValidateLibraryAndSource(sfxSource, "PlaySFX")) return;
        var clip = soundLibrary.GetSFX(sfx);
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    ///
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public bool ToggleSFX()
    {
        if (sfxSource == null) return false;
        sfxSource.mute = !sfxSource.mute;
        return sfxSource.mute;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private void PlayMusicClip(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    private void TryLoadLibrary()
    {
        if (soundLibrary != null) return;
        soundLibrary = Resources.Load<SoundLibrary>("SoundLibrary");
        if (soundLibrary == null)
            Debug.LogWarning("[AudioManager] Không tìm thấy SoundLibrary. Hãy tạo và gán vào Inspector hoặc đặt vào Resources/SoundLibrary.");
    }

    private void TryAutoAssignSources()
    {
        if (musicSource != null && sfxSource != null) return;
        var sources = gameObject.GetComponentsInChildren<AudioSource>();
        if (musicSource == null && sources.Length > 0) musicSource = sources[0];
        if (sfxSource   == null && sources.Length > 1) sfxSource   = sources[1];
    }

    private bool ValidateLibraryAndSource(AudioSource source, string caller)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning($"[AudioManager.{caller}] SoundLibrary chưa được gán.");
            return false;
        }
        if (source == null)
        {
            Debug.LogWarning($"[AudioManager.{caller}] AudioSource chưa được gán.");
            return false;
        }
        return true;
    }
}
