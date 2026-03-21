using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────
// Thêm entry mới ở đây khi có âm thanh mới.
// Không cần chỉnh sửa bất kỳ script gameplay nào khác.
// ─────────────────────────────────────────────────────────────
public enum MusicTrack
{
    None = 0,
    BackGround,
    MainMenu,
    Battle,
    Boss,
}

public enum SFXClip
{
    None = 0,
    PlayerFootstep,
    PlayerAttack,
    EnemyHit,
    EnemyDie,
    DoorOpen,
    DoorClose,
    ItemPickup,
    ChestOpen,
    UIClick,
}

// ─────────────────────────────────────────────────────────────
// Mapping entry: gán AudioClip cho từng enum value trong Inspector
// ─────────────────────────────────────────────────────────────
[Serializable]
public class MusicEntry
{
    public MusicTrack track;
    public AudioClip clip;
}

[Serializable]
public class SFXEntry
{
    public SFXClip sfx;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Header("Music")]
    public List<MusicEntry> musics = new List<MusicEntry>();

    [Header("SFX")]
    public List<SFXEntry> sfxs = new List<SFXEntry>();

    [Header("Startup")]
    [Tooltip("Track nhạc tự động phát khi game khởi động.")]
    public MusicTrack startupTrack = MusicTrack.BackGround;

    // ── Lookup helpers ────────────────────────────────────────
    public AudioClip GetMusic(MusicTrack track)
    {
        var entry = musics.Find(e => e.track == track);
        if (entry == null)
            Debug.LogWarning($"[SoundLibrary] Music track '{track}' chưa được gán clip.");
        return entry?.clip;
    }

    public AudioClip GetSFX(SFXClip sfx)
    {
        var entry = sfxs.Find(e => e.sfx == sfx);
        if (entry == null)
            Debug.LogWarning($"[SoundLibrary] SFX '{sfx}' chưa được gán clip.");
        return entry?.clip;
    }
}
