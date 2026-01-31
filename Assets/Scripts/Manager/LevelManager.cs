using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public static event Action OnRoomCompleted;
    public static event Action<float> OnPlayerInRoomBoss;

    public GameObject SelectedPlayer { get; set; }

    [Header("Templates")]
    //[SerializeField] private RoomTemplate roomTemplates;
    //[SerializeField] private DungeonLibrary dungeonLibrary;
    // public RoomTemplate RoomTemplates => roomTemplates;
    //public DungeonLibrary DoorSO => dungeonLibrary;

    [Header("Dungeon Config")]
    [SerializeField] private DungeonLevelSO startingDungeonLevel;
    [SerializeField] private DungeonLevelSO[] dungeonLevels;

    private int currentDungeonIndex = 0;

    private Room currentRoom;
    private DungeonLevelSO currentDungeonLevel;
    private int amountOfEnemies;
    private GameObject currentDungeonGO;

    private List<PickableItem> itemsInTheLevel = new List<PickableItem>();


    protected override void Awake()
    {
        base.Awake();
        CreatePlayerInDungeon();
        // Initialize current dungeon level from provided list or fallback to startingDungeonLevel
        if (dungeonLevels != null && dungeonLevels.Length > 0)
        {
            currentDungeonIndex = 0;
            currentDungeonLevel = dungeonLevels[currentDungeonIndex];
        }
        else
        {
            currentDungeonLevel = startingDungeonLevel;
        }
        DungeonBuilder dungeonBuilder = GameObject.FindObjectOfType<DungeonBuilder>();

    }
    private void Start()
    {
        CreateLevel();
    }
    // todo: fix sau
    private void CreateLevel()
    {
        if (currentDungeonLevel == null)
        {
            Debug.LogError("LevelManager: Missing startingDungeonLevel");
            return;
        }
        DungeonBuilder dungeonBuilder = DungeonBuilder.Instance;
        dungeonBuilder.LoadRoomNodeTypeList();
        bool built = dungeonBuilder.GenerateDungeon(currentDungeonLevel);
        if (!built)
        {
            Debug.LogError("LevelManager: Dungeon build failed");
            return;
        }

        PositionOfPlayerInDungeon();
    }
    public string GetCurrentLevelText()
    {
        //return $"Level {dungeonLibrary.levels[currentLevelIndex].levelName} - {dungeonLibrary.levels.Length}";
        return "Fix sau";
    }
    private void CreatePlayerInDungeon()
    {
        if(GameManager.Instance.playerPrefab != null)
        {
            SelectedPlayer =  Instantiate(GameManager.Instance.playerPrefab.playerPrefab);
            PlayerConfig player = SelectedPlayer.GetComponent<PlayerController>().PlayerData;
            SetStatWhenStart(player);
        }
    }
    //sau dung Json doc data
    public void SetStatWhenStart(PlayerConfig playerConfig)
    {
        playerConfig.currentHealth = playerConfig.MaxHealth;
        playerConfig.currentArmor = playerConfig.MaxArmor;
        playerConfig.currentEnergy = playerConfig.MaxEnergy;
    }

    private void CreateChestWhenCompleted()
    {
        if (currentRoom == null || currentRoom.instantiatedRoom == null)
            return;

        // Chọn vị trí ngẫu nhiên từ spawn positions của room
        if (currentRoom.spawnPositionArray != null && currentRoom.spawnPositionArray.Length > 0)
        {
            Vector2Int randomSpawnPos = currentRoom.spawnPositionArray[UnityEngine.Random.Range(0, currentRoom.spawnPositionArray.Length)];
            Vector3 chestPos = currentRoom.instantiatedRoom.grid.CellToWorld(new Vector3Int(randomSpawnPos.x, randomSpawnPos.y, 0));
            
            // Lấy chest prefab từ resources hoặc GameResources
            GameObject chestPrefab = Resources.Load<GameObject>("Chest");
            if (chestPrefab == null)
            {
                GameResources gameResources = Resources.Load<GameResources>("GameResources");
                if (gameResources != null && gameResources.chestItemPrefab != null)
                    chestPrefab = gameResources.chestItemPrefab;
            }
            
            if (chestPrefab != null)
            {
                Instantiate(chestPrefab, chestPos, Quaternion.identity, currentRoom.instantiatedRoom.transform);
            }
        }
    }

    private void PortalEventCallBack()
    {
        StartCoroutine(IEContinueDungeon());
    }

    public void PositionOfPlayerInDungeon()
    {
        // Move player to the entrance room if available
        if (SelectedPlayer == null) return;

        Room entrance = DungeonBuilder.Instance.GetEntranceRoom();
        if (entrance != null && entrance.instantiatedRoom != null)
        {
            var entrancePos = entrance.instantiatedRoom.transform.position;
            SelectedPlayer.transform.position = entrancePos;
        }
        AudioManager.Instance.PlayMusic("Theme");
    }

    private void ContinueNextLevel()
    {
        DungeonBuilder db = DungeonBuilder.Instance;

        // If we have a list of levels, advance index; otherwise just regenerate current level
        if (dungeonLevels != null && dungeonLevels.Length > 0)
        {
            // If not the last level -> build next
            if (currentDungeonIndex < dungeonLevels.Length - 1)
            {
                currentDungeonIndex++;
                currentDungeonLevel = dungeonLevels[currentDungeonIndex];

                db.ClearDungeonRuntime();
                db.LoadRoomNodeTypeList();
                bool built = db.GenerateDungeon(currentDungeonLevel);
                if (!built)
                {
                    Debug.LogError("LevelManager: Dungeon build failed on ContinueNextLevel");
                    return;
                }

                PositionOfPlayerInDungeon();
            }
            else
            {
                // Final level completed -> clear dungeon, reset player, clear pools and return to home
                db.ClearDungeonRuntime();
                Debug.Log("LevelManager: Final level completed. Returning to Home Scene.");
                ResetPlayerStats();
                ClearAllObjectPools();
                ReturnToHomeScene();
            }
        }
        else
        {
            // No level list provided: just rebuild the same level
            db.ClearDungeonRuntime();
            db.LoadRoomNodeTypeList();
            bool built = db.GenerateDungeon(currentDungeonLevel);
            if (!built)
            {
                Debug.LogError("LevelManager: Dungeon build failed on ContinueNextLevel (regen)");
                return;
            }
            PositionOfPlayerInDungeon();
        }
    }


    private IEnumerator IEContinueDungeon()
    {
        // Disable player control during transition
        if (SelectedPlayer != null)
        {
            var pc = SelectedPlayer.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;
        }

        UIManager.Instance.FadeNewDungeon(1);
        yield return new WaitForSeconds(2f);
        ContinueNextLevel();
        UIManager.Instance.UpdateLevelText(GetCurrentLevelText());
        UIManager.Instance.FadeNewDungeon(0f);

        // Re-enable player control after transition (if player still exists)
        yield return null; // wait one frame to ensure scene objects updated
        if (SelectedPlayer != null)
        {
            var pc = SelectedPlayer.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = true;
        }
    }

    public GameObject RandomItemInEachChest()
    {
        int randomIndex = UnityEngine.Random.Range(0, itemsInTheLevel.Count);
        return itemsInTheLevel[randomIndex].gameObject;
    }

    private void EnemyKilledBack(Transform enemyPos)
    {
        if (currentRoom == null) return;

        // Tạo bonus khi enemy chết
        CreateBonus(enemyPos);
        
        // Đếm số enemy còn lại trong room
        EnemyVitality[] remainingEnemies = currentRoom.instantiatedRoom.GetComponentsInChildren<EnemyVitality>();
        amountOfEnemies = remainingEnemies.Length;
        
        // Nếu không còn enemy trong room
        if (amountOfEnemies <= 0)
        {
            OnRoomCompleted?.Invoke();
            
            // Tạo chest ở vị trí ngẫu nhiên trong room
            CreateChestWhenCompleted();
        }
    }


    private void CreateBonus(Transform enemyPos)
    {
        if (currentRoom == null || currentRoom.prefab == null)
            return;

        // Get room template SO từ Room
        RoomTemplateSO roomTemplate = GetRoomTemplateForRoom(currentRoom);
        if (roomTemplate == null || roomTemplate.bonusPrefabs == null || roomTemplate.bonusPrefabs.Length == 0)
            return;

        int amount = UnityEngine.Random.Range(roomTemplate.minBonusPerKill, roomTemplate.maxBonusPerKill + 1);
        
        for (int i = 0; i < amount; i++)
        {
            int bonusRandom = UnityEngine.Random.Range(0, roomTemplate.bonusPrefabs.Length);
            Vector3 bonusOffset = UnityEngine.Random.insideUnitCircle.normalized * roomTemplate.bonusSpreadRadius;
            Vector3 spawnPos = enemyPos.position + bonusOffset;
            
            Instantiate(roomTemplate.bonusPrefabs[bonusRandom], spawnPos, Quaternion.identity, currentRoom.instantiatedRoom.transform);
        }
    }

    /// <summary>
    /// Lấy RoomTemplateSO từ Room object
    /// </summary>
    private RoomTemplateSO GetRoomTemplateForRoom(Room room)
    {
        if (room == null || currentDungeonLevel == null)
            return null;

        // Tìm room template trong roomTemplateList của current level
        foreach (RoomTemplateSO template in currentDungeonLevel.roomTemplateList)
        {
            if (template != null && template.prefab == room.prefab)
            {
                return template;
            }
        }

        return null;
    }

    private void OnEnable()
    {
        // Room.OnPlayerEnterTheRoom += PlayerEnterRoom;
        EnemyVitality.OnEnemyKilledEvent += EnemyKilledBack;
        Portal.OnNextDungeon += PortalEventCallBack;
        StaticEventHandler.OnRoomEnemiesDefeated += HandleRoomCleared;
    }

    private void OnDisable()
    {
        // Room.OnPlayerEnterTheRoom -= PlayerEnterRoom;
        EnemyVitality.OnEnemyKilledEvent -= EnemyKilledBack;
        Portal.OnNextDungeon -= PortalEventCallBack;
        StaticEventHandler.OnRoomEnemiesDefeated -= HandleRoomCleared;
    }

    private void HandleRoomCleared(Room room)
    {
        // Update current room reference
        currentRoom = room;
        
        // Đếm số enemy trong room mới
        if (room != null && room.instantiatedRoom != null)
        {
            EnemyVitality[] enemies = room.instantiatedRoom.GetComponentsInChildren<EnemyVitality>();
            amountOfEnemies = enemies.Length;
        }
    }

    public DungeonLevelSO GetCurrentDungeonLevel() => currentDungeonLevel;

    private void ResetPlayerStats()
    {
        if (SelectedPlayer == null) return;

        PlayerController pc = SelectedPlayer.GetComponent<PlayerController>();
        if (pc == null) return;

        PlayerConfig playerConfig = pc.PlayerData;
        if (playerConfig == null) return;

        // Reset all player stats to max values
        playerConfig.currentHealth = playerConfig.MaxHealth;
        playerConfig.currentArmor = playerConfig.MaxArmor;
        playerConfig.currentEnergy = playerConfig.MaxEnergy;
        playerConfig.timeCooldownArmor = 0;

        Debug.Log("LevelManager: Player stats reset to default values.");
    }

    private void ClearAllObjectPools()
    {
        if (ObjPoolManager.Instance != null)
        {
            ObjPoolManager.Instance.ClearAllPool();
            Debug.Log("LevelManager: All object pools cleared.");
        }
    }

    private void ReturnToHomeScene()
    {
        Debug.Log("LevelManager: Loading Home Scene.");
        DestroyPlayer();
        UnityEngine.SceneManagement.SceneManager.LoadScene(Settings.homeScene);
    }

    private void DestroyPlayer()
    {
        if (SelectedPlayer != null)
        {
            Destroy(SelectedPlayer);
            SelectedPlayer = null;
            Debug.Log("LevelManager: Player destroyed.");
        }
    }
}
