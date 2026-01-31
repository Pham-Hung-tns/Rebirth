using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens to room change events and spawns enemies based on RoomTemplate settings.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    private readonly Dictionary<Room, SpawnState> activeSpawns = new Dictionary<Room, SpawnState>();

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += HandleRoomChanged;
        EnemyVitality.OnEnemyKilledEvent += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= HandleRoomChanged;
        EnemyVitality.OnEnemyKilledEvent -= HandleEnemyKilled;
    }

    private void HandleRoomChanged(RoomChangedEventArgs args)
    {
        var room = args.room;
        Debug.Log($"[EnemySpawner] HandleRoomChanged called for roomId={room?.id}");

        if (room == null || room.roomNodeType == null)
        {
            Debug.Log("[EnemySpawner] Aborting HandleRoomChanged: room is null or room.roomNodeType is null");
            return;
        }

        if (room.roomNodeType.isCorridor || room.roomNodeType.isEntrance)
        {
            Debug.Log($"[EnemySpawner] Aborting HandleRoomChanged: room {room.id} is corridor or entrance");
            return;
        }

        if (room.isClearedOfEnemies)
        {
            Debug.Log($"[EnemySpawner] Aborting HandleRoomChanged: room {room.id} is already cleared of enemies");
            return;
        }

        DungeonLevelSO currentLevel = LevelManager.Instance?.GetCurrentDungeonLevel();
        if (currentLevel == null)
        {
            Debug.Log("[EnemySpawner] Aborting HandleRoomChanged: current dungeon level is null");
            return;
        }

        RoomEnemySpawnParameters spawnParams = room.GetRoomEnemySpawnParameters(currentLevel);
        if (spawnParams == null)
        {
            Debug.Log($"[EnemySpawner] Aborting HandleRoomChanged: spawnParams null for room {room.id} at level {currentLevel.name}");
            return;
        }

        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
        {
            Debug.Log($"[EnemySpawner] Aborting HandleRoomChanged: room {room.id} has no spawn positions");
            return;
        }

        if (!activeSpawns.ContainsKey(room))
        {
            var state = new SpawnState(room, spawnParams, currentLevel);
            activeSpawns[room] = state;

            // Lock doors and switch to battle music when combat starts
            Debug.Log($"[EnemySpawner] Conditions met. Locking doors for room {room.id} and starting spawn routine.");
            room.instantiatedRoom?.LockDoors();
            AudioManager.Instance.PlaySFX("Door_Close");
            AudioManager.Instance.PlayMusic("Battle");

            StartCoroutine(SpawnRoutine(state));
        }
    }

    private IEnumerator SpawnRoutine(SpawnState state)
    {
        var room = state.Room;
        int totalToSpawn = Random.Range(state.Parameters.minTotalEnemiesToSpawn, state.Parameters.maxTotalEnemiesToSpawn + 1);
        int maxConcurrent = Random.Range(state.Parameters.minConcurrentEnemies, state.Parameters.maxConcurrentEnemies + 1);

        int spawned = 0;
        while (spawned < totalToSpawn)
        {
            while (state.AliveCount >= maxConcurrent)
                yield return null;

            SpawnEnemy(state);
            spawned++;

            float interval = Random.Range(state.Parameters.minSpawnInterval, state.Parameters.maxSpawnInterval + 1);
            yield return new WaitForSeconds(interval);
        }

        // Wait for all enemies to die before clearing
        while (state.AliveCount > 0)
            yield return null;

        room.isClearedOfEnemies = true;

        // Unlock doors and restore ambient/theme music
        room.instantiatedRoom?.UnlockDoors(Settings.doorUnlockDelay);
        AudioManager.Instance.PlaySFX("Door_Open");
        AudioManager.Instance.PlayMusic("Theme");

        StaticEventHandler.CallRoomEnemiesDefeated(room);
        activeSpawns.Remove(room);
    }

    private void SpawnEnemy(SpawnState state)
    {
        EnemyDetailsSO enemyDetails = SelectRandomEnemy(state.Room, state.DungeonLevel);
        if (enemyDetails == null || enemyDetails.enemyPrefab == null) return;

        Vector3 spawnWorldPos = GetRandomSpawnWorldPosition(state.Room);
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, spawnWorldPos, Quaternion.identity, state.Room.instantiatedRoom?.transform);

        if (enemy != null)
        {
            // Configure before temporarily deactivating
            var vitality = enemy.GetComponent<EnemyVitality>();
            if (vitality != null)
            {
                int health = GetHealthForLevel(enemyDetails, state.DungeonLevel, (int)vitality.Health);
                vitality.Health = health;
            }

            // Attach context for cleanup
            var context = enemy.AddComponent<SpawnContext>();
            context.SourceRoom = state.Room;
            state.AliveCount++;

            // Temporarily deactivate the enemy to freeze behavior, then activate after short delay
            enemy.SetActive(false);
            float delay = Random.Range(0.5f, 1f);
            SpawnActivationHelper.Instance.ActivateAfterDelay(enemy, delay);
            Debug.Log($"[EnemySpawner] Spawned enemy {enemy.name} (delayed active {delay:F2}s) in room {state.Room.id}");
        }
    }

    private EnemyDetailsSO SelectRandomEnemy(Room room, DungeonLevelSO level)
    {
        if (room.enemiesByLevelList == null) return null;
        SpawnableObjectsByLevel<EnemyDetailsSO> byLevel = room.enemiesByLevelList.Find(e => e.dungeonLevel == level);
        if (byLevel == null || byLevel.spawnableObjectRatioList == null || byLevel.spawnableObjectRatioList.Count == 0)
            return null;

        int totalWeight = 0;
        foreach (var ratio in byLevel.spawnableObjectRatioList)
            totalWeight += Mathf.Max(0, ratio.ratio);

        if (totalWeight <= 0) return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var ratio in byLevel.spawnableObjectRatioList)
        {
            cumulative += Mathf.Max(0, ratio.ratio);
            if (roll < cumulative)
                return ratio.dungeonObject;
        }

        return byLevel.spawnableObjectRatioList[0].dungeonObject;
    }

    private Vector3 GetRandomSpawnWorldPosition(Room room)
    {
        Vector2Int cell = room.spawnPositionArray[Random.Range(0, room.spawnPositionArray.Length)];
        Vector3 world = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(cell.x + room.lowerBounds.x, cell.y + room.lowerBounds.y, 0));
        world += new Vector3(0.5f, 0.5f, 0f);
        return world;
    }

    private int GetHealthForLevel(EnemyDetailsSO details, DungeonLevelSO level, int fallback)
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

    private void HandleEnemyKilled(Transform enemyTransform)
    {
        if (enemyTransform == null) return;
        var ctx = enemyTransform.GetComponent<SpawnContext>();
        if (ctx == null || ctx.SourceRoom == null) return;

        if (activeSpawns.TryGetValue(ctx.SourceRoom, out var state))
        {
            state.AliveCount = Mathf.Max(0, state.AliveCount - 1);
        }
    }

    private class SpawnState
    {
        public Room Room { get; }
        public RoomEnemySpawnParameters Parameters { get; }
        public DungeonLevelSO DungeonLevel { get; }
        public int AliveCount;

        public SpawnState(Room room, RoomEnemySpawnParameters parameters, DungeonLevelSO level)
        {
            Room = room;
            Parameters = parameters;
            DungeonLevel = level;
            AliveCount = 0;
        }
    }

    private class SpawnContext : MonoBehaviour
    {
        public Room SourceRoom;
    }
}

