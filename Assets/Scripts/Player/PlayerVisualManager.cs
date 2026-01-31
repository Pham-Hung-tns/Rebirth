using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages player visual components (FogVision, Light2D, Minimap Icon)
/// Deactivates them in HomeScene and activates them in DungeonScene
/// </summary>
public class PlayerVisualManager : MonoBehaviour
{
    [Header("Player Visual Components")]
    [SerializeField] private GameObject fogVision;
    [SerializeField] private GameObject light2D;
    [SerializeField] private GameObject minimapIcon;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Check current scene on start
        string sceneName = SceneManager.GetActiveScene().name;
        UpdateVisualComponents(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisualComponents(scene.name);
    }

    private void UpdateVisualComponents(string sceneName)
    {
        bool isInDungeon = sceneName == "DungeonScene";

        if (fogVision != null)
            fogVision.SetActive(isInDungeon);

        if (light2D != null)
            light2D.SetActive(isInDungeon);

        if (minimapIcon != null)
            minimapIcon.SetActive(isInDungeon);

        Debug.Log($"[PlayerVisualManager] Scene: {sceneName} - Visual components active: {isInDungeon}");
    }
}
