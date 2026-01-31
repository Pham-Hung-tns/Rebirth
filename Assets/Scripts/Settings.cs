using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region SCENE SETTINGS
    public const string homeScene = "HomeScene";
    #endregion

    #region ROOM SETTINGS
    public const float fadeInTime = 0.5f; // time to fade in the room
    public const int maxChildCorridors = 3; // Max number of child corridors leading from a room.
    public const float doorUnlockDelay = 1f;
    #endregion

    #region ANIMATOR PLAYER PARAMETERS
    // Animator parameters - Player
    public static readonly int PLAYER_IDLE = Animator.StringToHash("Player_Idle");
    public static readonly int PLAYER_RUN = Animator.StringToHash("Player_Run");
    public static readonly int PLAYER_SKILL = Animator.StringToHash("Player_Skill");
    public const float baseSpeedForPlayerAnimations = 8f;

    // Animator parameters - Enemy
    #region ANIMATOR ENEMY PARAMETERS
      public const int defaultEnemyVitality = 20;
    public static readonly int IDLE_STATE = Animator.StringToHash("Enemy_Idle");
    public static readonly int WANDER_STATE = Animator.StringToHash("Enemy_Wander");
    public static readonly int ATTACK_STATE = Animator.StringToHash("Enemy_Attack");
    public static readonly int CHASE_STATE = Animator.StringToHash("Enemy_Chase");
    #endregion


    // // Animator parameters - Door
    // public static readonly int open = Animator.StringToHash("Open");
    // public static readonly int close = Animator.StringToHash("Close");

    // Animator parameters - DamageableDecoration
    public static readonly int destroy = Animator.StringToHash("destroy");
    public static readonly string stateDestroyed = "Destroyed";
    #endregion

    #region ANIMATOR WEAPON PARAMETERS
    // Animator parameters - Weapon: Bow
    public static readonly int BOW_IDLE = Animator.StringToHash("Bow_Idle");
    public static readonly int BOW_CHARGING = Animator.StringToHash("Bow_Charging");
    public static readonly int BOW_RELEASE = Animator.StringToHash("Bow_Release");

    #endregion

    #region GAMEOBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    #endregion

    #region ROOM TILEMAP TAGS
    public const string groundTilemapTag = "groundTilemap";
    public const string decoration1TilemapTag = "decoration1Tilemap";
    public const string decoration2TilemapTag = "decoration2Tilemap";
    public const string frontTilemapTag = "frontTilemap";
    public const string collisionTilemapTag = "collisionTilemap";
    public const string minimapTilemapTag = "minimapTilemap";
    #endregion

    #region AUDIO
    public const float musicFadeOutTime = 0.5f;  // Defualt Music Fade Out Transition
    public const float musicFadeInTime = 0.5f;  // Default Music Fade In Transition
    #endregion

    #region FIRING CONTROL
    public const float useAimAngleDistance = 3.5f; // if the target distance is less than this then the aim angle will be used (calculated from player), else the weapon aim angle will be used (calculated from the weapon). 
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const int targetFrameRateToSpreadPathfindingOver = 60;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;
    #endregion


    #region UI PARAMETERS
    public const float uiHeartSpacing = 16f;
    public const float uiAmmoIconSpacing = 4f;
    #endregion

    #region CONTACT DAMAGE PARAMETERS
    public const float contactDamageCollisionResetDelay = 0.5f;
    #endregion

    #region HIGHSCORES
    public const int numberOfHighScoresToSave = 100;
    #endregion

}
