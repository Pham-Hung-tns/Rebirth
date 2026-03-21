using UnityEngine;

/// <summary>
/// Detect device type và toggle mobile-only UI controls.
/// Đặt trên Canvas chứa mobile controls.
/// Trong Inspector, kéo joystick/fireBtn/skillBtn/changeWeaponBtn vào mobileOnlyElements[].
/// </summary>
public class PlatformUIController : MonoBehaviour
{
    [Header("Mobile-Only Controls")]
    [Tooltip("GameObjects chỉ hiển thị trên mobile (joystick, fire, skill, changeWeapon)")]
    [SerializeField] private GameObject[] mobileOnlyElements;

    [Header("Desktop-Only Controls")]
    [Tooltip("GameObjects chỉ hiển thị trên desktop/web")]
    [SerializeField] private GameObject[] desktopOnlyElements;

    [Header("Editor Testing")]
    [Tooltip("Bật để force mobile UI trong Editor (không cần đổi build target)")]
    [SerializeField] private bool forceMobileInEditor = false;

    public static bool IsMobile { get; private set; }

    private void Awake()
    {
        DetectPlatform();
    }

    private void DetectPlatform()
    {
#if UNITY_EDITOR
        // Ưu tiên forceMobileInEditor nếu bật, nếu không thì detect từ build target
        if (forceMobileInEditor)
        {
            IsMobile = true;
        }
        else
        {
            IsMobile = UnityEditor.EditorUserBuildSettings.activeBuildTarget
                is UnityEditor.BuildTarget.Android
                or UnityEditor.BuildTarget.iOS;
        }
#else
        IsMobile = Application.isMobilePlatform;
#endif
    }

    private void ApplyPlatformUI()
    {
        if (mobileOnlyElements != null)
        {
            foreach (var element in mobileOnlyElements)
            {
                if (element != null)
                    element.SetActive(IsMobile);
            }
        }

        if (desktopOnlyElements != null)
        {
            foreach (var element in desktopOnlyElements)
            {
                if (element != null)
                    element.SetActive(!IsMobile);
            }
        }
    }

    /// <summary>
    /// Gọi khi cần bật UI mobile sau khi game-state thay đổi (ví dụ: sau khi chọn character).
    /// Chỉ bật nếu platform thực sự là mobile.
    /// </summary>
    public void ShowMobileControls()
    {
        if (!IsMobile || mobileOnlyElements == null) return;
        foreach (var element in mobileOnlyElements)
            if (element != null) element.SetActive(true);
    }

    public void ShowDesktopControls()
    {
        if (!IsMobile || desktopOnlyElements == null) return;
        foreach (var element in desktopOnlyElements)
            if (element != null) element.SetActive(true);
    }
}
