using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BigMapController : MonoBehaviour
{
    [Header("References")]
    public Camera bigMapCamera;
    public float padding = 5f;

    [Header("Input References (optional)")]
    // Có thể gán 1 trong các trường sau trong Inspector
    [Tooltip("If you use the generated PlayerControls class, assign it here (optional)")]
    [SerializeField] public PlayerControls playerInput;
    [Tooltip("If you use a PlayerInput component on your player GameObject, assign it here (optional)")]
    [SerializeField] public UnityEngine.InputSystem.PlayerInput playerInputComponent;
    [Tooltip("If you prefer to assign the InputActionAsset directly, assign it here (optional)")]
    [SerializeField] public UnityEngine.InputSystem.InputActionAsset playerActionAsset;
    [Tooltip("Reference to the InputReader ScriptableObject used by your game (optional)")]
    [SerializeField] public InputReader inputReader;

    public string gameplayActionMap = "Player"; // Tên Action Map điều khiển nhân vật (kiểm tra trong file Input Actions của bạn)

    [Header("Data (Manual Assign)")]
    // Bạn kéo thả các Room (GameObject cha) vào list này trong Inspector
    public List<Transform> allRooms; 

    // Hàm này sẽ được gọi bởi Button Minimap
    public void ToggleBigMap()
    {
        // Nếu đang tắt -> Bật lên và Fit Camera
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            bigMapCamera.gameObject.SetActive(true);
            FitCameraToDungeon();
            // Prefer calling the game's shared InputReader if available so we affect the actual input instance
            if (inputReader != null)
            {
                inputReader.SetGameplayEnabled(false);
            }
            else
            {
                var asset = GetPlayerActionAsset();
                if (asset == null)
                {
                    Debug.LogWarning("No player action asset found. Assign one to BigMapController or use a PlayerInput on scene.");
                }
                else
                {
                    InputActionMap gameplayMap = asset.FindActionMap(gameplayActionMap);
                    if (gameplayMap != null) gameplayMap.Disable();
                    else Debug.LogWarning($"Action map '{gameplayActionMap}' not found on asset.");
                }
            }
        }
        // Nếu đang bật -> Tắt đi
        else
        {
            CloseBigMap();
            if (inputReader != null)
            {
                inputReader.SetGameplayEnabled(true);
            }
            else
            {
                var asset = GetPlayerActionAsset();
                if (asset == null)
                {
                    Debug.LogWarning("No player action asset found. Assign one to BigMapController or use a PlayerInput on scene.");
                }
                else
                {
                    InputActionMap gameplayMap = asset.FindActionMap(gameplayActionMap);
                    if (gameplayMap != null) gameplayMap.Enable();
                    else Debug.LogWarning($"Action map '{gameplayActionMap}' not found on asset.");
                }
            }
        }
    }

    private UnityEngine.InputSystem.InputActionAsset GetPlayerActionAsset()
    {
        if (playerActionAsset != null) return playerActionAsset;
        if (playerInput != null) return playerInput.asset;
        if (playerInputComponent != null) return playerInputComponent.actions;

        // Try to find a PlayerInput in scene (first hit)
        var p = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        if (p != null) return p.actions;

        return null;
    }

    public void CloseBigMap()
    {
        gameObject.SetActive(false);
        bigMapCamera.gameObject.SetActive(false);
    }

    private void FitCameraToDungeon()
    {
        if (allRooms == null || allRooms.Count == 0)
        {
            Debug.LogWarning("Chưa gán Room nào vào list allRooms!");
            return;
        }

        // Khởi tạo Bounds bằng null để xử lý logic chính xác hơn
        Bounds dungeonBounds = new Bounds();
        bool hasBounds = false;

        foreach (var room in allRooms)
        {
            // Tìm tất cả các Renderer (SpriteRenderer, TilemapRenderer...) trong room con
            // Đặc biệt là cái MinimapGraphic (Sprite hình vuông)
            Renderer[] renderers = room.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                Debug.Log(r.gameObject.name + " - Layer: " + LayerMask.LayerToName(r.gameObject.layer));
                // Chỉ tính toán bounds của các object thuộc layer Minimap để chính xác nhất
                // (Nếu bạn lười check layer thì bỏ dòng if này cũng được, nhưng check thì chuẩn hơn)
                if (r.gameObject.layer == LayerMask.NameToLayer("MiniMap")) 
                {
                    if (!hasBounds)
                    {
                        dungeonBounds = r.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        dungeonBounds.Encapsulate(r.bounds);
                    }
                }
            }
        }

        if (!hasBounds) 
        {
            // Fallback: Nếu không tìm thấy renderer nào thì dùng vị trí tâm như cũ
            dungeonBounds = new Bounds(allRooms[0].position, Vector3.zero);
            foreach (var room in allRooms) dungeonBounds.Encapsulate(room.position);
        }

        // --- ĐOẠN DƯỚI NÀY GIỮ NGUYÊN ---
        
        // Đặt Camera vào giữa
        Vector3 center = dungeonBounds.center;
        center.z = bigMapCamera.transform.position.z;
        bigMapCamera.transform.position = center;

        // Tính toán Zoom (Orthographic Size)
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = dungeonBounds.size.x / dungeonBounds.size.y;

        if (screenRatio >= targetRatio)
        {
            // Fit chiều dọc
            bigMapCamera.orthographicSize = (dungeonBounds.size.y / 2f) + padding;
        }
        else
        {
            // Fit chiều ngang
            float differenceInSize = targetRatio / screenRatio;
            bigMapCamera.orthographicSize = (dungeonBounds.size.y / 2f * differenceInSize) + padding;
        }
    }
}