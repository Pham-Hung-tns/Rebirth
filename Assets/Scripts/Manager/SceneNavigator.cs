using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Xử lý chuyển scene. Tách ra từ UISystem để đảm bảo single responsibility.
/// Gắn vào GameObject riêng trên mỗi scene cần chức năng navigation.
/// </summary>
public class SceneNavigator : MonoBehaviour
{
    public void StartGame()
    {
        if (Time.timeScale == 0) return;
        SceneManager.LoadScene(Settings.HOME_SCENE);
    }

    public void ReturnToStart()
    {
        GameManager.Instance.playerPrefab = null;
        SceneManager.LoadScene(Settings.START_SCENE);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
