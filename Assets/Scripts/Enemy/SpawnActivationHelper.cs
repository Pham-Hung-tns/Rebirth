using System.Collections;
using UnityEngine;

/// <summary>
/// Utility singleton to handle delayed activation of instantiated enemies.
/// Use `SpawnActivationHelper.Instance.ActivateAfterDelay(go, delay)` to schedule activation.
/// </summary>
public class SpawnActivationHelper : MonoBehaviour
{
    private static SpawnActivationHelper _instance;
    public static SpawnActivationHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SpawnActivationHelper");
                _instance = go.AddComponent<SpawnActivationHelper>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void ActivateAfterDelay(GameObject obj, float delay)
    {
        StartCoroutine(ActivateCoroutine(obj, delay));
    }

    private IEnumerator ActivateCoroutine(GameObject obj, float delay)
    {
        if (obj == null)
            yield break;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (obj != null)
            obj.SetActive(true);
    }
}
