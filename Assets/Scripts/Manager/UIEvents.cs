using System;
using UnityEngine;

[Serializable]
public struct PlayerStatsData
{
    public float curHp, maxHp, curArmor, maxArmor, curEnergy, maxEnergy;
}

public static class UIEvents
{
    // Player stats: curHP, maxHP, curArmor, maxArmor, curEnergy, maxEnergy
    public static Action<PlayerStatsData> OnPlayerStatsChanged;

    // total coins
    public static Action<float> OnCoinChanged;

    // Show weapon UI
    public static Action<Weapon> OnShowWeapon;

    // Start skill cooldown (duration in seconds)
    public static Action<float> OnStartSkillCooldown;

    // Pickup button toggle
    public static Action<bool> OnPickupToggle;

    // Level text update
    public static Action<string> OnLevelTextUpdate;

    // Room completed
    public static Action OnRoomCompleted;

    // Boss health updated (0..1)
    public static Action<float> OnBossHealthUpdated;

    // Fade dungeon (value)
    public static Action<float> OnFadeNewDungeon;

    // Show game over
    public static Action OnShowGameOver;
    
    // Bắt đầu một đoạn hội thoại mới, truyền trực tiếp NPCInteractable
    public static Action<NPCInteractable> OnStartDialogue;

    // Bật/tắt trạng thái mở Popup
    // (True: đang có Panel mở -> Khóa Input Player, False: Đã tắt hết Panel -> Mở lại Input)
    public static Action<bool> OnUIStateChanged;

    // Quản lý Stack UI
    public static Action<GameObject, bool> OnPushPopup;
    public static Action<GameObject> OnPopPopup;
}