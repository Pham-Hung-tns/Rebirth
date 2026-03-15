using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Helper class ?? spawn enemies v� chests trong room
/// D�ng cho c? editor mode (TestDungeonBuilderEditor) v� runtime mode (LevelManager)
/// </summary>
public class RoomContentSpawner
{
    /// <summary>
    /// Spawn enemies trong m?t room
    /// </summary>
    public static int SpawnEnemiesInRoom(Room room, DungeonLevelSO dungeonLevel)
    {
        if (room == null || room.instantiatedRoom == null)
        {
            Debug.LogWarning("Room ho?c instantiatedRoom l� null, kh�ng th? spawn enemy");
            return 0;
        }

        // Skip corridor v� entrance
        if (room.roomNodeType.isCorridor || room.roomNodeType.isEntrance)
            return 0;

        // Ki?m tra spawn position
        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
        {
            Debug.LogWarning($"Room {room.id} kh�ng c� spawn positions");
            return 0;
        }

        // Ki?m tra enemy data
        if (room.enemiesByLevelList == null || room.enemiesByLevelList.Count == 0)
        {
            Debug.LogWarning($"Room {room.id} kh�ng c� enemy data");
            return 0;
        }

        // L?y spawn parameters
        RoomEnemySpawnParameters spawnParams = room.GetRoomEnemySpawnParameters(dungeonLevel);
        if (spawnParams == null)
        {
            Debug.LogWarning($"Room {room.id} kh�ng c� spawn parameters cho level {dungeonLevel.name}");
            return 0;
        }

        // T�nh s? l??ng enemy c?n spawn
        int totalToSpawn = Random.Range(spawnParams.minTotalEnemiesToSpawn, spawnParams.maxTotalEnemiesToSpawn + 1);

        int enemiesSpawned = 0;
        for (int i = 0; i < totalToSpawn; i++)
        {
            EnemyConfig enemyDetails = SelectRandomEnemy(room, dungeonLevel);
            if (enemyDetails == null || enemyDetails.enemyPrefab == null)
                continue;

            Vector3 spawnWorldPos = GetRandomSpawnWorldPosition(room);

            // Lấy enemy từ pool (nếu có), fallback Instantiate cho editor mode
            GameObject enemy;
            if (ObjPoolManager.Instance != null)
            {
                enemy = ObjPoolManager.Instance.GetFromPool(
                    enemyDetails.enemyPrefab, spawnWorldPos, Quaternion.identity,
                    room.instantiatedRoom.transform);
            }
            else
            {
                enemy = Object.Instantiate(enemyDetails.enemyPrefab, spawnWorldPos, Quaternion.identity, room.instantiatedRoom.transform);
                enemy.name = enemyDetails.enemyPrefab.name;
            }

            if (enemy != null)
            {
                enemy.name = $"{enemyDetails.name}_{enemiesSpawned}";

                // Áp dụng health theo level nếu có
                EnemyVitality vitality = enemy.GetComponent<EnemyVitality>();
                if (vitality != null)
                {
                    int health = GetHealthForLevel(enemyDetails, dungeonLevel, (int)vitality.Health);
                    vitality.Health = health;
                }

                // Rebuild room grid cache cho pathfinding (enemy pooled cần cập nhật room mới)
                EnemyMovement emMovement = enemy.GetComponent<EnemyMovement>();
                if (emMovement != null)
                {
                    // Set tilemaps trước khi rebuild
                    if (emMovement.GroundTilemap == null && room.instantiatedRoom.groundTilemap != null)
                        emMovement.GroundTilemap = room.instantiatedRoom.groundTilemap;
                    if (emMovement.CollisionTilemap == null && room.instantiatedRoom.collisionTilemap != null)
                        emMovement.CollisionTilemap = room.instantiatedRoom.collisionTilemap;
                    emMovement.RebuildRoomGridCache();
                }

                // Temporarily deactivate enemy so it doesn't act immediately; activate after short delay
                enemy.SetActive(false);
                float delay = Random.Range(0.5f, 1f);
                SpawnActivationHelper.Instance.ActivateAfterDelay(enemy, delay);
                Debug.Log($"[RoomContentSpawner] Spawned enemy {enemy.name} - activation delayed by {delay:F2}s in room {room.id}");

                enemiesSpawned++;
            }
        }

        if (enemiesSpawned > 0)
        {
            Debug.Log($"Spawned {enemiesSpawned} enemies trong room {room.id}");
        }

        return enemiesSpawned;
    }

    /// <summary>
    /// Spawn chests trong m?t room
    /// </summary>
    public static int SpawnChestsInRoom(Room room, DungeonLevelSO dungeonLevel)
    {
        if (room == null || room.instantiatedRoom == null)
            return 0;

        // Skip corridor v� entrance
        if (room.roomNodeType.isCorridor || room.roomNodeType.isEntrance)
            return 0;

        // Ki?m tra spawn position
        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
            return 0;

        // Lấy chest prefab từ DungeonLevelSO
        GameObject chestPrefab = dungeonLevel?.chestPrefab;

        if (chestPrefab == null)
        {
            Debug.LogWarning("Không tìm thấy chest prefab trong DungeonLevelSO");
            return 0;
        }

        // Roll x�c su?t spawn chest (25-50%)
        float chance = Random.Range(0.25f, 0.5f);
        if (Random.value > chance)
            return 0;

        // Ch?n v? tr� spawn
        Vector3 spawnWorldPos = GetRandomSpawnWorldPosition(room);
        GameObject chestGO = Object.Instantiate(chestPrefab, spawnWorldPos, Quaternion.identity, room.instantiatedRoom.transform);
        if (chestGO != null)
        {
            Chest chest = chestGO.GetComponent<Chest>();
            if (chest != null && dungeonLevel != null && dungeonLevel.chestItem != null)
            {
                chest.SetChestItemData(dungeonLevel.chestItem);
            }
            Debug.Log($"Spawned chest trong room {room.id}");
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Ch?n enemy ng?u nhi�n d?a tr�n weighted ratio
    /// </summary>
    private static EnemyConfig SelectRandomEnemy(Room room, DungeonLevelSO level)
    {
        if (room.enemiesByLevelList == null)
            return null;

        SpawnableObjectsByLevel<EnemyConfig> byLevel = room.enemiesByLevelList.Find(e => e.dungeonLevel == level);
        if (byLevel == null || byLevel.spawnableObjectRatioList == null || byLevel.spawnableObjectRatioList.Count == 0)
            return null;

        int totalWeight = 0;
        foreach (var ratio in byLevel.spawnableObjectRatioList)
            totalWeight += Mathf.Max(0, ratio.ratio);

        if (totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var ratio in byLevel.spawnableObjectRatioList)
        {
            cumulative += Mathf.Max(0, ratio.ratio);
            if (roll < cumulative)
                return ratio.dungeonObject;
        }

        return byLevel.spawnableObjectRatioList.Count > 0 ? byLevel.spawnableObjectRatioList[0].dungeonObject : null;
    }

    /// <summary>
    /// L?y v? tr� spawn world t? spawn position array
    /// </summary>
    private static Vector3 GetRandomSpawnWorldPosition(Room room)
    {
        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
            return room.instantiatedRoom.transform.position;

        Vector2Int spawnCell = room.spawnPositionArray[Random.Range(0, room.spawnPositionArray.Length)];

        // spawnCell l� local coordinates c?a room template
        // C�ng th?c: worldPos = spawnCell + room.lowerBounds - room.templateLowerBounds
        Vector3 worldPos = new Vector3(
            spawnCell.x + room.lowerBounds.x - room.templateLowerBounds.x,
            spawnCell.y + room.lowerBounds.y - room.templateLowerBounds.y,
            0f
        );

        // Center c?a tile
        worldPos += new Vector3(0.5f, 0.5f, 0f);

        return worldPos;
    }

    /// <summary>
    /// L?y health d?a theo level
    /// </summary>
    private static int GetHealthForLevel(EnemyConfig details, DungeonLevelSO level, int fallback)
    {
        if (details.healthByLevel != null)
        {
            foreach (var h in details.healthByLevel)
            {
                if (h != null && h.dungeonLevel == level)
                    return h.health;
            }
        }
        return fallback;
    }
}
