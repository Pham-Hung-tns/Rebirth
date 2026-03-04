using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Persistence<GameManager>
{
    public Color weaponNormalColor;
     public Color weaponRareColor;
     public Color weaponEpicColor;
     public Color weaponLegendColor;
    public GameData gameData;
    public PlayerConfig playerPrefab;
    protected override void Awake()
    {
        base.Awake();
        gameData = SaveSystem.Load();
    }

    public Color ChooseColorForWeapon(WeaponDataSO weapon)
    {
        switch (weapon.weaponRarity)
        {
            case WeaponDataSO.WeaponRarity.Normal:
                return weaponNormalColor;
            case WeaponDataSO.WeaponRarity.Rare:
                return weaponRareColor;
            case WeaponDataSO.WeaponRarity.Epic:
                return weaponEpicColor;
            case WeaponDataSO.WeaponRarity.Legend:
                return weaponLegendColor;
        }
        return weaponNormalColor;
    }
    
}
