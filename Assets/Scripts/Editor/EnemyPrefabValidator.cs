#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Editor utility to validate EnemyConfig assets and their prefabs
public static class EnemyPrefabValidator
{
    [MenuItem("Tools/Validate Enemy Prefabs")]
    public static void ValidateEnemyPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyConfig");
        int errors = 0;
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (asset == null) continue;
            if (asset.enemyPrefab == null)
            {
                Debug.LogWarning($"EnemyConfig '{asset.name}' has no enemyPrefab assigned ({path}).");
                errors++;
                continue;
            }

            var prefab = asset.enemyPrefab;
            var em = prefab.GetComponent<EnemyMovement>();
            var ec = prefab.GetComponent<EnemyController>();
            if (em == null)
            {
                Debug.LogWarning($"Prefab '{prefab.name}' referenced by '{asset.name}' is missing EnemyMovement component.");
                errors++;
            }
            if (ec == null)
            {
                Debug.LogWarning($"Prefab '{prefab.name}' referenced by '{asset.name}' is missing EnemyController component.");
                errors++;
            }
        }

        if (errors == 0)
            EditorUtility.DisplayDialog("Enemy Prefab Validation", "No issues found for EnemyConfig prefabs.", "OK");
        else
            EditorUtility.DisplayDialog("Enemy Prefab Validation", $"Validation completed with {errors} warnings.", "OK");
    }
}
#endif