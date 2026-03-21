using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Settings
{
    #region UNITS
    public const float PIXELS_PER_UNIT = 16f;
    public const float TILE_SIZE_PIXELS = 16f;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int MAX_DUNGEON_REBUILD_ATTEMPTS_FOR_ROOM_GRAPH = 1000;
    public const int MAX_DUNGEON_BUILD_ATTEMPTS = 10;
    #endregion

    #region SCENE SETTINGS
    public const string HOME_SCENE = "HomeScene";
    public const string START_SCENE = "StartScene";
    public const string GAME_SCENE = "DungeonScene";
    #endregion

    #region ROOM SETTINGS
    public const float FADE_IN_TIME = 0.5f; // time to fade in the room
    public const int MAX_CHILD_CORRIDORS = 3; // Max number of child corridors leading from a room.
    public const float DOOR_UNLOCK_DELAY = 1f;
    #endregion

    #region ANIMATOR PLAYER PARAMETERS
    // Animator parameters - Player
    public static readonly int PLAYER_IDLE = Animator.StringToHash("Player_Idle");
    public static readonly int PLAYER_RUN = Animator.StringToHash("Player_Run");
    public static readonly int PLAYER_SKILL = Animator.StringToHash("Player_Skill");
    public const float BASE_SPEED_FOR_PLAYER_ANIMATIONS = 8f;
    #endregion

    // Animator parameters - Enemy
    #region ANIMATOR ENEMY PARAMETERS
      public const int DEFAULT_ENEMY_VITALITY = 20;
    public static readonly int IDLE_STATE = Animator.StringToHash("Enemy_Idle");
    public static readonly int WANDER_STATE = Animator.StringToHash("Enemy_Wander");
    public static readonly int ATTACK_STATE = Animator.StringToHash("Enemy_Attack");
    public static readonly int CHASE_STATE = Animator.StringToHash("Enemy_Chase");
    #endregion

    #region ANIMATOR WEAPON PARAMETERS
    // Animator parameters - Weapon: Bow
    public static readonly int BOW_IDLE = Animator.StringToHash("Bow_Idle");
    public static readonly int BOW_CHARGING = Animator.StringToHash("Bow_Charging");
    public static readonly int BOW_RELEASE = Animator.StringToHash("Bow_Release");

    #endregion

    #region GAMEOBJECT TAGS
    public const string PLAYER_TAG = "Player";
    public const string PLAYER_WEAPON = "playerWeapon";
    #endregion

    #region ROOM TILEMAP TAGS
    public const string GROUND_TILEMAP_TAG = "groundTilemap";
    public const string DECORATION1_TILEMAP_TAG = "decoration1Tilemap";
    public const string DECORATION2_TILEMAP_TAG = "decoration2Tilemap";
    public const string FRONT_TILEMAP_TAG = "frontTilemap";
    public const string COLLISION_TILEMAP_TAG = "collisionTilemap";
    public const string MINIMAP_TILEMAP_TAG = "minimapTilemap";
    #endregion

    #region AUDIO
    public const float MUSIC_FADE_OUT_TIME = 0.5f;  // Defualt Music Fade Out Transition
    public const float MUSIC_FADE_IN_TIME = 0.5f;  // Default Music Fade In Transition
    #endregion

    #region FIRING CONTROL
    public const float USE_AIM_ANGLE_DISTANCE = 3.5f; // if the target distance is less than this then the aim angle will be used (calculated from player), else the weapon aim angle will be used (calculated from the weapon). 
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int DEFAULT_ASTAR_MOVEMENT_PENALTY = 40;
    public const int PREFERRED_PATH_ASTAR_MOVEMENT_PENALTY = 1;
    public const int TARGET_FRAME_RATE_TO_SPREAD_PATHFINDING_OVER = 60;
    public const float PLAYER_MOVE_DISTANCE_TO_REBUILD_PATH = 3f;
    public const float ENEMY_PATH_REBUILD_COOLDOWN = 2f;
    #endregion

    #region GATE ANIMATION
    public static readonly string GATE_OPEN = "Open";
    #endregion

}
