using UnityEngine;

public class AudioFeedback : Feedback
{
    [Header("Audio Config")]
    [Tooltip("Kéo thả file âm thanh (AudioClip) vào đây (Ví dụ: tiếng súng bắn, tiếng quái thú gầm...)")]
    [SerializeField] private AudioClip clip;
    
    [Range(0f, 1f)]
    [Tooltip("Âm lượng phát ra (0 đến 1)")]
    [SerializeField] private float volume = 1f;

    public override void CreateFeedback()
    {
        if (clip == null) return;
        
        CompletePreviousFeedback();
        
        // Sử dụng ngay SfxSource từ GameManager/AudioManager để phát âm thanh mà không cần tạo AudioSource mới
        if (AudioManager.Instance != null && AudioManager.Instance.SfxSource != null)
        {
            AudioManager.Instance.SfxSource.PlayOneShot(clip, volume);
        }
    }

    public override void CompletePreviousFeedback()
    {
        // OneShot sẽ tự động dọn dẹp bộ nhớ và tự tắt khi kêu xong, 
        // nên chúng ta không cần gượng ép ngắt quãng nó giữa chừng ở đây.
    }
}
