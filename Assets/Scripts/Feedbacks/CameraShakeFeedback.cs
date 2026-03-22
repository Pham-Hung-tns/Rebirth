using UnityEngine;

public class CameraShakeFeedback : Feedback
{
    [Header("Camera Shake Config")]
    [Tooltip("Sức mạnh độ giật (Intensity) của Camera")]
    [SerializeField] private float intensity = 5f;
    
    [Tooltip("Thời gian rung (giây)")]
    [SerializeField] private float duration = 0.2f;

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();
        
        if (CameraManager.Instance != null)
        {
            // Truyền lệnh giật màn hình xuống CameraManager
            CameraManager.Instance.ShakeCM(intensity, duration);
        }
    }

    public override void CompletePreviousFeedback()
    {
        // Animation rung của Cinemachine đã tự nội suy trơn tru theo thời gian (duration)
        // Nên chúng ta thường không cần ngắt giữa chừng ở đây.
    }
}
