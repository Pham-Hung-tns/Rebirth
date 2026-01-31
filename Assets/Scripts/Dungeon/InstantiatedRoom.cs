using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty;  // use this 2d array to store movement penalties from the tilemaps to be used in AStar pathfinding
    [HideInInspector] public int[,] aStarItemObstacles; // use to store position of moveable items that are obstacles
    [HideInInspector] public Bounds roomColliderBounds;
    //[HideInInspector] public List<MoveItem> moveableItemsList = new List<MoveItem>();

    #region Header OBJECT REFERENCES

    [Space(10)]
    [Header("OBJECT REFERENCES")]

    #endregion Header OBJECT REFERENCES

    #region Tooltip

    [Tooltip("Populate with the environment child placeholder gameobject ")]

    #endregion Tooltip

    [SerializeField] private GameObject environmentGameObject;


    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;

    }

    private void Start()
    {
        // Update moveable item obstacles array
        //UpdateMoveableObstacles();
    }


    // Trigger room changed event when player enters a room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if player hasn't entered the room then return
        if (collision.tag != Settings.playerTag) return;

        Debug.Log($"[InstantiatedRoom] Player entered room trigger: roomId={room?.id} (previouslyVisited={room?.isPreviouslyVisited}) - Collider={collision.name}");

        // if room has already been visited then return
        if (room.isPreviouslyVisited)
        {
            Debug.Log($"[InstantiatedRoom] Room {room.id} was already visited - skipping spawn and lock.");
            return;
        }

        // Set room as visited
        room.isPreviouslyVisited = true;

        // Get current dungeon level
        DungeonLevelSO dungeonLevel = LevelManager.Instance.GetCurrentDungeonLevel();
        if (dungeonLevel == null)
        {
            Debug.LogWarning($"[InstantiatedRoom] Current dungeon level is null for room {room.id} - aborting spawn.");
            return;
        }

        Debug.Log($"[InstantiatedRoom] Spawning content for room {room.id} at level {dungeonLevel.name}.");

        // Spawn enemies and chests
        RoomContentSpawner.SpawnEnemiesInRoom(room, dungeonLevel);
        RoomContentSpawner.SpawnChestsInRoom(room, dungeonLevel);

        // Call room changed event
        Debug.Log($"[InstantiatedRoom] Calling StaticEventHandler.CallRoomChangedEvent for room {room.id}.");
        StaticEventHandler.CallRoomChangedEvent(room);
    }

    /// <summary>
    /// Initialise The Instantiated Room
    /// </summary>
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);
        

        BlockOffUnusedDoorWays();

        /// <summary>
        /// Thêm các chướng ngại vật vào phòng để tạo thử thách
        /// Tạo các đường đi ưa thích cho kẻ địch
        /// Thiết lập các điểm di chuyển cho AI

        //AddObstaclesAndPreferredPaths();

        /// </summary>
        
//         Mục đích: Tạo mảng các chướng ngại vật cho vật phẩm
// Chức năng:
// Tạo một mảng để lưu trữ các chướng ngại vật
// Thiết lập các vị trí không thể đặt vật phẩm
// Đảm bảo vật phẩm không bị đặt ở vị trí không hợp lệ
        //CreateItemObstaclesArray();

        AddDoorsToRooms();

        //DisableCollisionTilemapRenderer();

    }

    /// <summary>
    /// Populate the tilemap and grid memeber variables.
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        // Get the grid component.
        grid = roomGameobject.GetComponentInChildren<Grid>();

        // Get tilemaps in children.
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == Settings.groundTilemapTag)
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == Settings.decoration1TilemapTag)
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == Settings.decoration2TilemapTag)
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == Settings.frontTilemapTag)
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == Settings.collisionTilemapTag)
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == Settings.minimapTilemapTag)
            {
                minimapTilemap = tilemap;
            }
            else
            {
                continue;
            }

        }

    }

    /// <summary>
    /// Block Off Unused Doorways In The Room
    /// </summary>
    private void BlockOffUnusedDoorWays()
    {
        // Loop through all doorways
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected)
                continue;
            else
            {
                Debug.Log(doorway.orientation);
            }
            // Block unconnected doorways using tiles on tilemaps
            if (collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }

            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }

            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }

            if (decoration2Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }

            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Block a doorway on a tilemap layer
    /// </summary>
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;

            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;
        }

    }

    /// <summary>
    /// Block doorway horizontally - for North and South doorways
    /// </summary>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Block doorway vertically - for East and West doorways
    /// </summary>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {

            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);

            }

        }
    }

    /// <summary>
    /// Update obstacles used by AStar pathfinmding.
    /// </summary>
    private void AddObstaclesAndPreferredPaths()
    {
        // this array will be populated with wall obstacles 
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];


        // Loop thorugh all grid squares
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                // Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                // Add preferred path for enemies (1 is the preferred path value, default value for
                // a grid location is specified in the Settings).
                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }

            }
        }

    }


    /// <summary>
    /// Add opening doors if this is not a corridor room
    /// </summary>
    private void AddDoorsToRooms()
    {
        // if the room is a corridor then return
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;

        // Instantiate door prefabs at doorway positions
        foreach (Doorway doorway in room.doorWayList)
        {

            // if the doorway prefab isn't null and the doorway is connected
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                // Compute centered local position for the door prefab based on tile coords
                // Convert tile coordinates to local units and place prefab center on the tile
                Vector3 centeredPos = new Vector3((doorway.position.x + 0.5f) * tileDistance, (doorway.position.y + 0.5f) * tileDistance, 0f);

                if (doorway.orientation == Orientation.north)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = centeredPos;
                }
                else if (doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = centeredPos;
                }
                else if (doorway.orientation == Orientation.east)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = centeredPos;
                }
                else if (doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = centeredPos;
                }

                // Preserve the prefab's scale
                if (door != null)
                {
                    door.transform.localScale = doorway.doorPrefab.transform.localScale;
                }

                // // Get door component
                // Door doorComponent = door.GetComponent<Door>();

                // // Set if door is part of a boss room
                // if (room.roomNodeType.isBossRoom)
                // {
                //     doorComponent.isBossRoomDoor = true;

                //     // lock the door to prevent access to the room
                //     doorComponent.LockDoor();

                //     // Instantiate skull icon for minimap by door
                //     GameObject skullIcon = Instantiate(GameResources.Instance.minimapSkullPrefab, gameObject.transform);
                //     skullIcon.transform.localPosition = door.transform.localPosition;

                // }
            }

        }

    }


    /// <summary>
    /// Disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        // Disable collision tilemap renderer
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;

    }

    /// <summary>
    /// Disable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    /// <summary>
    /// Enable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    public void ActivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(true);
    }

    public void DeactivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(false);
    }


    /// <summary>
    /// Lock the room doors
    /// </summary>
    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        Debug.Log($"[InstantiatedRoom] LockDoors called for room {room?.id}. Found {doorArray.Length} door(s).");

        // Trigger lock doors
        foreach (Door door in doorArray)
        {
            Debug.Log($"[InstantiatedRoom] Locking door: {door.gameObject.name}");
            door.LockDoor();
        }

        // Disable room trigger collider
        DisableRoomCollider();
    }

    /// <summary>
    /// Unlock the room doors
    /// </summary>
    public void UnlockDoors(float doorUnlockDelay = 0f)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    /// <summary>
    /// Unlock the room doors routine
    /// </summary>
    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);
        Door[] doorArray = GetComponentsInChildren<Door>();

        Debug.Log($"[InstantiatedRoom] UnlockDoorsRoutine called for room {room?.id} after delay {doorUnlockDelay}. Found {doorArray.Length} door(s).");

        // Trigger open doors
        foreach (Door door in doorArray)
        {
            Debug.Log($"[InstantiatedRoom] Unlocking door: {door.gameObject.name}");
            door.UnlockDoor();
        }

        // Enable room trigger collider
        EnableRoomCollider();
    }

    /// <summary>
    /// Create Item Obstacles Array
    /// </summary>
    private void CreateItemObstaclesArray()
    {
        // this array will be populated during gameplay with any moveable obstacles
        aStarItemObstacles = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];
    }

    /// <summary>
    /// Initialize Item Obstacles Array With Default AStar Movement Penalty Values
    /// </summary>
    private void InitializeItemObstaclesArray()
    {
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarItemObstacles[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }
    }

    /// <summary>
    /// Update the array of moveable obstacles
    /// </summary>
    // public void UpdateMoveableObstacles()
    // {
    //     InitializeItemObstaclesArray();

    //     foreach (MoveItem moveItem in moveableItemsList)
    //     {
    //         Vector3Int colliderBoundsMin = grid.WorldToCell(moveItem.boxCollider2D.bounds.min);
    //         Vector3Int colliderBoundsMax = grid.WorldToCell(moveItem.boxCollider2D.bounds.max);

    //         // Loop through and add moveable item collider bounds to obstacle array
    //         for (int i = colliderBoundsMin.x; i <= colliderBoundsMax.x; i++)
    //         {
    //             for (int j = colliderBoundsMin.y; j <= colliderBoundsMax.y; j++)
    //             {
    //                 aStarItemObstacles[i - room.templateLowerBounds.x, j - room.templateLowerBounds.y] = 0;
    //             }
    //         }
    //     }
    // }

    ///// <summary>
    ///// This is used for debugging - shows the position of the table obstacles. 
    ///// (MUST BE COMMENTED OUT BEFORE UPDATING ROOM PREFABS)
    ///// </summary>
    //private void OnDrawGizmos()
    //{

    //    for (int i = 0; i < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); i++)
    //    {
    //        for (int j = 0; j < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); j++)
    //        {
    //            if (aStarItemObstacles[i, j] == 0)
    //            {
    //                Vector3 worldCellPos = grid.CellToWorld(new Vector3Int(i + room.templateLowerBounds.x, j + room.templateLowerBounds.y, 0));

    //                Gizmos.DrawWireCube(new Vector3(worldCellPos.x + 0.5f, worldCellPos.y + 0.5f, 0), Vector3.one);
    //            }
    //        }
    //    }

    //}


    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(environmentGameObject), environmentGameObject);
    }

#endif

    #endregion Validation
}