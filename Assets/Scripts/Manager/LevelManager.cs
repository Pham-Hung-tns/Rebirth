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

    [Header("Dungeon Config")]
    [SerializeField] private DungeonLevelSO startingDungeonLevel;
    [SerializeField] private DungeonLevelSO[] dungeonLevels;

    private int currentDungeonIndex = 0;

    private Room currentRoom;
    private DungeonLevelSO currentDungeonLevel;

    private GameObject currentDungeonGO;

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
            SelectedPlayer.GetComponent<PlayerWeapon>().enabled = true;
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

        // Phát nhạc nền
        AudioManager.Instance.PlayMusic(MusicTrack.BackGround);

        // Đánh dấu rằng player đã được đặt vào màn chơi mới
        // Và cho phép phát âm thanh di chuyển (tiếng bước chân)
        var playerController = SelectedPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerPlacedInLevel(true);
        }
    }

    private void ContinueNextLevel()
    {
        DungeonBuilder db = DungeonBuilder.Instance;

        // Tạm dừng âm thanh trước khi chuyển sang màn chơi mới
        AudioManager.Instance.PauseAudio();

        // Đánh dấu rằng player chưa được đặt vào màn chơi mới
        // Điều này sẽ ngăn chặn phát âm thanh di chuyển cho đến khi player được đặt xong
        var playerController = SelectedPlayer != null ? SelectedPlayer.GetComponent<PlayerController>() : null;
        if (playerController != null)
        {
            playerController.SetPlayerPlacedInLevel(false);
        }

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

        UIEvents.OnFadeNewDungeon?.Invoke(1f);
        yield return new WaitForSeconds(2f);
        ContinueNextLevel();  // ContinueNextLevel sẽ tạm dừng âm thanh
        UIEvents.OnLevelTextUpdate?.Invoke(GetCurrentLevelText());
        UIEvents.OnFadeNewDungeon?.Invoke(0f);

        // Tiếp tục phát âm thanh sau khi định vị player xong
        AudioManager.Instance.ResumeAudio();

        // Re-enable player control after transition (if player still exists)
        yield return null; // wait one frame to ensure scene objects updated
        if (SelectedPlayer != null)
        {
            var pc = SelectedPlayer.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = true;
        }
    }



    private void EnemyKilledBack(Transform enemyPos)
    {
        if (currentRoom == null) return;

        // Tạo bonus khi enemy chết
        CreateBonus(enemyPos);
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
            
            GameObject bonus;
            if (ObjPoolManager.Instance != null)
            {
                bonus = ObjPoolManager.Instance.GetFromPool(roomTemplate.bonusPrefabs[bonusRandom], spawnPos, Quaternion.identity, currentRoom.instantiatedRoom.transform);
            }
            else
            {
                bonus = Instantiate(roomTemplate.bonusPrefabs[bonusRandom], spawnPos, Quaternion.identity, currentRoom.instantiatedRoom.transform);
            }
            bonus.SetActive(true);
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
        EnemyVitality.OnEnemyKilledEvent += EnemyKilledBack;
        Portal.OnNextDungeon += PortalEventCallBack;
        StaticEventHandler.OnRoomEnemiesDefeated += HandleRoomCleared;
        StaticEventHandler.OnRoomChanged += HandleRoomChangedForLevelManager;
    }

    private void OnDisable()
    {
        EnemyVitality.OnEnemyKilledEvent -= EnemyKilledBack;
        Portal.OnNextDungeon -= PortalEventCallBack;
        StaticEventHandler.OnRoomEnemiesDefeated -= HandleRoomCleared;
        StaticEventHandler.OnRoomChanged -= HandleRoomChangedForLevelManager;
    }

    private void HandleRoomChangedForLevelManager(RoomChangedEventArgs args)
    {
        currentRoom = args.room;
    }

    private void HandleRoomCleared(Room room)
    {
        // Thông báo UI khi room đã được dọn sạch enemy
        OnRoomCompleted?.Invoke();
        UIEvents.OnRoomCompleted?.Invoke();

        // Spawn chest khi room cleared
        SpawnChestInRoom(room);
    }

    private void SpawnChestInRoom(Room room)
    {
        if (room == null || room.instantiatedRoom == null)
            return;

        if (room.spawnPositionArray == null || room.spawnPositionArray.Length == 0)
            return;

        // Roll xác suất spawn chest (25-50%)
        float chance = UnityEngine.Random.Range(0.25f, 0.5f);
        if (UnityEngine.Random.value > chance)
        {
            return;
        }

        // Lấy chest prefab từ DungeonLevelSO
        if (currentDungeonLevel == null || currentDungeonLevel.chestPrefab == null)
        {

            return;
        }

        GameObject chestPrefab = currentDungeonLevel.chestPrefab;

        // Chọn vị trí spawn ngẫu nhiên
        Vector2Int spawnCell = room.spawnPositionArray[UnityEngine.Random.Range(0, room.spawnPositionArray.Length)];
        Vector3 spawnPos = new Vector3(
            spawnCell.x + room.lowerBounds.x - room.templateLowerBounds.x,
            spawnCell.y + room.lowerBounds.y - room.templateLowerBounds.y,
            0f
        );
        spawnPos += new Vector3(0.5f, 0.5f, 0f);

        // Instantiate chest
        GameObject chestGO = Instantiate(chestPrefab, spawnPos, Quaternion.identity, room.instantiatedRoom.transform);
        if (chestGO != null)
        {
            Chest chest = chestGO.GetComponent<Chest>();
            if (chest != null && currentDungeonLevel != null && currentDungeonLevel.chestItem != null)
            {
                chest.SetChestItemData(currentDungeonLevel.chestItem);
            }

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


    }

    private void ClearAllObjectPools()
    {
        if (ObjPoolManager.Instance != null)
        {
            ObjPoolManager.Instance.ClearAllPool();

        }
    }

    private void ReturnToHomeScene()
    {

        DestroyPlayer();
        UnityEngine.SceneManagement.SceneManager.LoadScene(Settings.HOME_SCENE);
    }

    private void DestroyPlayer()
    {
        if (SelectedPlayer != null)
        {
            Destroy(SelectedPlayer);
            SelectedPlayer = null;

        }
    }
}
