using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Audio control UI — bật/tắt/điều chỉnh âm lượng.
/// Hoạt động ở mọi scene (instance riêng mỗi scene).
/// </summary>
public class UISystem : MonoBehaviour
{
    [Header("Audio Panel")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private Slider musicSlider, sfxSlider;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private Button musicButton, sfxButton;

    private void Start()
    {
       musicButton.image.color = AudioManager.Instance.MusicSource.mute == true ? Color.black : Color.white;
       sfxButton.image.color = AudioManager.Instance.SfxSource.mute == true ? Color.black : Color.white;
    }

    public void ShowAudioPanel()
    {
        pauseButton.SetActive(false);
        audioPanel.SetActive(true);
        Time.timeScale = 0;
    }
    public void HideAudioPanel()
    {
        pauseButton.SetActive(true);
        audioPanel.SetActive(false);
        Time.timeScale = 1;
    }
    public void ToggleMusic()
    {
        musicButton.image.color = AudioManager.Instance.ToggleMusic() == true ? Color.black : Color.white;
    }
    public void ToggleSFX()
    {
        sfxButton.image.color = AudioManager.Instance.ToggleSFX() == true ? Color.black : Color.white;
    }
    public void AdjustMusic()
    {
        AudioManager.Instance.SetMusicVolume(musicSlider.value);
    }
    public void AdjustSFX()
    {
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);
    }
}
