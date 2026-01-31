using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class TestDungeonBuilderEditor
{
    [MenuItem("Tools/Test Dungeon Builder")]
    public static void TestDungeonBuilder()
    {
        // Tìm DungeonBuilder trong scene
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            Debug.LogError("Không tìm thấy DungeonBuilder trong scene!");
            Debug.LogError("Sử dụng Tools/Setup Test Scene/Auto Setup Test Scene để tạo DungeonBuilder");
            return;
        }

        // Tìm DungeonLevelSO - thử từ Resources trước, nếu không có thì cho phép chọn
        DungeonLevelSO dungeonLevel = Resources.Load<DungeonLevelSO>("DungeonLevel_1-2");
        if (dungeonLevel == null)
        {
            // Tìm tất cả DungeonLevelSO trong project
            string[] guids = AssetDatabase.FindAssets("t:DungeonLevelSO");
            if (guids.Length == 0)
            {
                Debug.LogError("Không tìm thấy DungeonLevelSO nào trong project!");
                Debug.LogError("Vui lòng tạo DungeonLevelSO: Assets > Create > Scriptable Objects > Dungeon > Dungeon Level");
                return;
            }

            // Nếu có nhiều, chọn cái đầu tiên hoặc cho phép chọn
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            dungeonLevel = AssetDatabase.LoadAssetAtPath<DungeonLevelSO>(path);
            Debug.Log($"Tự động chọn DungeonLevelSO: {path}");
        }

        if (dungeonLevel == null)
        {
            Debug.LogError("Không thể load DungeonLevelSO!");
            return;
        }

        // Gọi hàm tạo map
        dungeonBuilder.LoadRoomNodeTypeList();
        bool result = dungeonBuilder.GenerateDungeon(dungeonLevel);
        Debug.Log("Kết quả tạo dungeon: " + result);
        if (!result)
            DebugLogRoomPlacements(dungeonBuilder);

        if (result)
        {
            // Spawn enemies và chests trong editor mode ngay sau khi tạo dungeon
            // Enemies sẽ được spawn trong tất cả các room (không cần player vào room)
            SpawnEnemiesInEditor(dungeonBuilder, dungeonLevel);
            SpawnChestsInEditor(dungeonBuilder, dungeonLevel);
            
            // Refresh scene view để hiển thị enemies
            SceneView.RepaintAll();
            
            Debug.Log("✅ Hoàn tất! Kiểm tra Scene View để xem dungeon và enemies.");
        }
    }

    [MenuItem("Tools/Test Dungeon Builder/Generate Map Only")]
    public static void TestDungeonBuilderMapOnly()
    {
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            Debug.LogError("Không tìm thấy DungeonBuilder trong scene!");
            return;
        }

        DungeonLevelSO dungeonLevel = Resources.Load<DungeonLevelSO>("DungeonLevel_1-2");
        if (dungeonLevel == null)
        {
            Debug.LogError("Không tìm thấy DungeonLevelSO trong Resources!");
            return;
        }

        dungeonBuilder.LoadRoomNodeTypeList();
        bool result = dungeonBuilder.GenerateDungeon(dungeonLevel);
        Debug.Log("Kết quả tạo dungeon: " + result);
        if (!result)
            DebugLogRoomPlacements(dungeonBuilder);

    }

    [MenuItem("Tools/Test Dungeon Builder/Spawn Enemies Only")]
    public static void SpawnEnemiesOnly()
    {
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            Debug.LogError("Không tìm thấy DungeonBuilder trong scene!");
            return;
        }

        DungeonLevelSO dungeonLevel = Resources.Load<DungeonLevelSO>("DungeonLevel_1-2");
        if (dungeonLevel == null)
        {
            Debug.LogError("Không tìm thấy DungeonLevelSO trong Resources!");
            return;
        }

        SpawnEnemiesInEditor(dungeonBuilder, dungeonLevel);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Test Dungeon Builder/Spawn Chests Only")]
    public static void SpawnChestsOnly()
    {
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            Debug.LogError("Không tìm thấy DungeonBuilder trong scene!");
            return;
        }

        DungeonLevelSO dungeonLevel = Resources.Load<DungeonLevelSO>("DungeonLevel_1-2");
        if (dungeonLevel == null)
        {
            Debug.LogError("Không tìm thấy DungeonLevelSO trong Resources!");
            return;
        }

        SpawnChestsInEditor(dungeonBuilder, dungeonLevel);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Test Dungeon Builder/Clear Dungeon")]
    public static void ClearDungeon()
    {
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();
        if (dungeonBuilder == null)
        {
            Debug.LogError("Không tìm thấy DungeonBuilder trong scene!");
            return;
        }

        if (EditorUtility.DisplayDialog("Clear Dungeon", "Bạn có chắc muốn xóa dungeon đã tạo không? Hành động này không thể hoàn tác.", "Xóa", "Hủy"))
        {
            dungeonBuilder.ClearDungeonForEditor();
            SceneView.RepaintAll();
            Debug.Log("🔥 Dungeon đã được dọn dẹp.");
        }
    }

    private static void SpawnEnemiesInEditor(DungeonBuilder dungeonBuilder, DungeonLevelSO dungeonLevel)
    {
        if (dungeonBuilder.dungeonBuilderRoomDictionary == null || dungeonBuilder.dungeonBuilderRoomDictionary.Count == 0)
        {
            Debug.LogWarning("Không có room nào trong dungeon để spawn enemy!");
            return;
        }

        int totalEnemiesSpawned = 0;
        foreach (var kvp in dungeonBuilder.dungeonBuilderRoomDictionary)
        {
            Room room = kvp.Value;
            totalEnemiesSpawned += RoomContentSpawner.SpawnEnemiesInRoom(room, dungeonLevel);
        }

        Debug.Log($"✅ Đã spawn {totalEnemiesSpawned} enemies trong editor mode.");
    }

    private static void SpawnChestsInEditor(DungeonBuilder dungeonBuilder, DungeonLevelSO dungeonLevel)
    {
        if (dungeonBuilder.dungeonBuilderRoomDictionary == null || dungeonBuilder.dungeonBuilderRoomDictionary.Count == 0)
        {
            Debug.LogWarning("Không có room nào trong dungeon để spawn chest!");
            return;
        }

        int totalChestsSpawned = 0;
        foreach (var kvp in dungeonBuilder.dungeonBuilderRoomDictionary)
        {
            Room room = kvp.Value;
            totalChestsSpawned += RoomContentSpawner.SpawnChestsInRoom(room, dungeonLevel);
        }

        Debug.Log($"✅ Đã spawn {totalChestsSpawned} chests trong editor mode.");
    }

    private static void DebugLogRoomPlacements(DungeonBuilder dungeonBuilder)
    {
        if (dungeonBuilder == null) return;

        var dict = dungeonBuilder.dungeonBuilderRoomDictionary;
        if (dict == null || dict.Count == 0) return;

        // Only log failures: when a Room is null or its instantiatedRoom is null
        foreach (var kvp in dict)
        {
            string key = kvp.Key != null ? kvp.Key.ToString() : "<null key>";
            Room room = kvp.Value;

            if (room == null)
            {
                Debug.LogError($"[Room {key}] Không thể đặt room: đối tượng Room là null.");
                continue;
            }

            if (room.instantiatedRoom == null)
            {
                string spawnInfo = (room.spawnPositionArray == null) ? "spawnPositions=null" : $"spawnPositions={room.spawnPositionArray.Length}";
                string roomName = "<no-name>";
                if (room.prefab != null && !string.IsNullOrEmpty(room.prefab.name))
                    roomName = room.prefab.name;
                else if (!string.IsNullOrEmpty(room.id))
                    roomName = room.id;
                else if (!string.IsNullOrEmpty(room.templateID))
                    roomName = room.templateID;

                Debug.LogError($"[Room {key} - {roomName}] Không thể instantiate room. lowerBounds={room.lowerBounds} templateLowerBounds={room.templateLowerBounds} {spawnInfo}");
            }
        }
    }

    private static EnemyDetailsSO SelectRandomEnemy(Room room, DungeonLevelSO level)
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

    private static Vector3 GetRandomSpawnWorldPosition(Room room)
    {
        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
            return room.instantiatedRoom.transform.position;

        Vector2Int spawnCell = room.spawnPositionArray[Random.Range(0, room.spawnPositionArray.Length)];
        
        // spawnCell là local coordinates của room template
        // Công thức chuyển đổi: worldPos = (spawnCell + room.lowerBounds - room.templateLowerBounds)
        // (tương tự như InstantiateRoomGameobjects)
        Vector3 worldPos = new Vector3(
            spawnCell.x + room.lowerBounds.x - room.templateLowerBounds.x,
            spawnCell.y + room.lowerBounds.y - room.templateLowerBounds.y,
            0f
        );
        
        // Center của tile
        worldPos += new Vector3(0.5f, 0.5f, 0f);
        
        return worldPos;
    }

    private static int GetHealthForLevel(EnemyDetailsSO details, DungeonLevelSO level, int fallback)
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