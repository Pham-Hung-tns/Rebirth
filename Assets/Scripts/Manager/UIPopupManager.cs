using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý Stack các UI Panel (như Dialogue, Menu, Settings).
/// Nhận lệnh bật/tắt Panel thông qua UIEvents độc lập hoàn toàn với GameplayUIManager.
/// </summary>
public class UIPopupManager : Singleton<UIPopupManager>
{
    private List<GameObject> activePopups = new List<GameObject>();
    private float previousTimeScale = 1f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        UIEvents.OnPushPopup += PushPopup;
        UIEvents.OnPopPopup += PopPopup;
    }

    private void OnDisable()
    {
        UIEvents.OnPushPopup -= PushPopup;
        UIEvents.OnPopPopup -= PopPopup;
    }

    private void PushPopup(GameObject popup, bool pauseGame)
    {
        if (popup == null) return;

        if (activePopups.Count == 0)
        {
            UIEvents.OnUIStateChanged?.Invoke(true); // Disable combat input
            if (pauseGame)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }
        else if (pauseGame && Time.timeScale != 0f)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        popup.SetActive(true);
        if (!activePopups.Contains(popup)) activePopups.Add(popup);
    }

    private void PopPopup(GameObject popup)
    {
        if (popup == null) return;

        if (activePopups.Contains(popup))
        {
            popup.SetActive(false);
            activePopups.Remove(popup);

            if (activePopups.Count == 0)
            {
                UIEvents.OnUIStateChanged?.Invoke(false); // Enable combat input
                Time.timeScale = previousTimeScale;
            }
        }
    }
}
