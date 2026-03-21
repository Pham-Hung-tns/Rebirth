using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class ClickHandle : MonoBehaviour
{
    void Update()
    {
        if (Time.timeScale == 0)
            return;

        // Nếu người chơi đã bấm nút Play/Start và chọn nhân vật xong thì tắt luôn tính năng click chọn này
        if (MenuManager.Instance != null && MenuManager.Instance.IsPlayerSelected)
            return;

        if (Input.GetMouseButtonDown(0)) // Cả chuột trái lẫn chạm màn hình cảm ứng
        {
            // Kiểm tra xem chuột/ngón tay có đang bấm trúng UI (như Màn hình chọn nhân vật, nâng cấp...) không
            if (IsPointerOverUI())
                return;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                // Kiểm tra Object đó có chứa script SelectCharacter không trước khi truyền đi (tránh màn hình báo lỗi Null)
                SelectCharacter obj = hit.transform.GetComponent<SelectCharacter>();
                if (obj != null)
                {
                    MenuManager.Instance.ShowStats(obj);
                }
            }
        }
    }

    /// <summary>
    /// Xử lý chống click đè mờ màn hình UI - hỗ trợ cả PC (chuột) lẫn Mobile (cảm ứng)
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Cho PC / Editor
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Cho Mobile Touch
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;
            }
        }

        return false;
    }
}
