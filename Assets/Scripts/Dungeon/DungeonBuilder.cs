using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;
    private Room entranceRoomCache;

    // private void OnEnable()
    // {
    //     // Set dimmed material to off
    //     GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    // }

    // private void OnDisable()
    // {
    //     // Set dimmed material to fully visible
    //     GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    // }

    protected override void Awake()
    {
        base.Awake();

        // Load the room node type list
        LoadRoomNodeTypeList();

    }


    /// <summary>
    /// Load the room node type list
    /// </summary>
    public void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Generate random dungeon, returns true if dungeon built, false if failed
    /// </summary>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        entranceRoomCache = null;

        // Load the scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // Select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // Loop until dungeon successfully built or more than max attempts for node graph
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // Clear dungeon room gameobjects and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // Attempt To Build A Random Dungeon For The Selected room node graph
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }


            if (dungeonBuildSuccessful)
            {
                // Instantiate Room Gameobjects
                InstantiateRoomGameobjects();
            }
        }

        return dungeonBuildSuccessful;
    }

    /// <summary>
    /// Load the room templates into the dictionary
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        // Clear room template dictionary
        roomTemplateDictionary.Clear();

        // Defensive checks to avoid null key crash
        if (roomTemplateList == null)
        {
            Debug.LogError("LoadRoomTemplatesIntoDictionary: roomTemplateList is null");
            return;
        }

        // Load room template list into dictionary
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate == null)
            {
                Debug.LogWarning("LoadRoomTemplatesIntoDictionary: encountered null RoomTemplateSO in roomTemplateList, skipping.");
                continue;
            }

            if (string.IsNullOrEmpty(roomTemplate.guid))
            {
                string prefabName = roomTemplate.prefab != null ? roomTemplate.prefab.name : "<null prefab>";
                Debug.LogWarning($"LoadRoomTemplatesIntoDictionary: RoomTemplateSO has null/empty guid (prefab={prefabName}), skipping.");
                continue;
            }

            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.LogWarning($"Duplicate Room Template Key In roomTemplateList: {roomTemplate.guid}");
            }
        }
    }

    /// <summary>
    /// Thử tạo dungeon ngẫu nhiên dựa trên roomNodeGraph đã cho
    /// Trả về true nếu tạo thành công, false nếu gặp vấn đề và cần thử lại
    /// </summary>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // Tạo hàng đợi các node phòng cần xử lý
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        var entranceType = roomNodeTypeList.list.Find(x => x.isEntrance);
        var entranceNode = roomNodeGraph.GetRoomNode(entranceType);
        // Thêm node cửa vào vào hàng đợi từ roomNodeGraph
        entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("Không tìm thấy node cửa vào");
            return false;  // Không tạo được dungeon
        }

        Debug.Log($"AttemptToBuildRandomDungeon: Selected room node graph={(roomNodeGraph!=null?roomNodeGraph.name:"<null>")}, entranceNode={(entranceNode!=null?entranceNode.id:"<null>")}");

        // Bắt đầu với giả định không có phòng nào chồng lấn
        bool noRoomOverlaps = true;

        // Xử lý các node trong hàng đợi
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        Debug.Log($"AttemptToBuildRandomDungeon: After processing queue. openRoomNodeQueue.Count={openRoomNodeQueue.Count}, noRoomOverlaps={noRoomOverlaps}, dungeonBuilderRoomDictionary.Count={dungeonBuilderRoomDictionary.Count}");

        // Nếu đã xử lý hết các node và không có phòng nào chồng lấn thì trả về true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Xử lý các phòng trong hàng đợi, trả về true nếu không có phòng nào chồng lấn
    /// </summary>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // Xử lý cho đến khi hết node trong hàng đợi hoặc phát hiện chồng lấn
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // Lấy node tiếp theo từ hàng đợi
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // Thêm các node con vào hàng đợi từ roomNodeGraph
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // Nếu là phòng cửa vào thì đánh dấu đã đặt vị trí và thêm vào dictionary
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                if (roomTemplate == null)
                {
                    Debug.LogError($"Không tìm thấy RoomTemplateSO cho loại phòng: {roomNode.roomNodeType.roomNodeTypeName}");
                }

                string templateInfo = roomTemplate != null ? roomTemplate.guid : "<null template>";
                string prefabInfo = (roomTemplate != null && roomTemplate.prefab != null) ? roomTemplate.prefab.name : "<null prefab>";
                Debug.Log($"Processing Entrance Node: nodeId={roomNode.id}, selectedTemplate={templateInfo}, prefab={prefabInfo}");

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                // mark as positioned for entrance - even if template/prefab had issues we avoid null derefs
                if (room != null)
                {
                    room.isPositioned = true;

                    // Thêm phòng vào dictionary (guard against duplicate keys)
                    if (!dungeonBuilderRoomDictionary.ContainsKey(room.id))
                    {
                        dungeonBuilderRoomDictionary.Add(room.id, room);
                    }
                    else
                    {
                        Debug.LogWarning($"AttemptToBuildRandomDungeon: Duplicate room id when adding entrance: {room.id}");
                    }

                    entranceRoomCache = room;
                }
                else
                {
                    Debug.LogError($"CreateRoomFromRoomTemplate returned null for entrance node {roomNode.id}");
                }
            }
            // Nếu không phải phòng cửa vào
            else
            {
                // Lấy phòng cha cho node - guard để tránh KeyNotFoundException
                Room parentRoom = null;
                string parentId = (roomNode.parentRoomNodeIDList != null && roomNode.parentRoomNodeIDList.Count > 0) ? roomNode.parentRoomNodeIDList[0] : "<no-parent-id>";
                if (!string.IsNullOrEmpty(parentId) && dungeonBuilderRoomDictionary.TryGetValue(parentId, out parentRoom))
                {
                    // Kiểm tra xem có thể đặt phòng mà không bị chồng lấn không
                    noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
                }
                else
                {
                    Debug.LogError($"ProcessRoomsInOpenRoomNodeQueue: Parent room with id {parentId} not found for node {roomNode.id}");
                    noRoomOverlaps = false;
                }
            }
        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// Thử đặt node phòng vào dungeon - nếu đặt được thì trả về true, ngược lại trả về false
    /// </summary>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // Khởi tạo và giả định có chồng lấn cho đến khi chứng minh ngược lại
        bool roomOverlaps = true;
        int placementAttempts = 0;

        // Do While Room Overlaps - try to place against all available doorways of the parent until
        // the room is successfully placed without overlap.
        while (roomOverlaps)
        {
            placementAttempts++;
            Debug.Log($"[CanPlaceRoomWithNoOverlaps] Attempt #{placementAttempts} for node {roomNode.id} (type: {roomNode.roomNodeType.roomNodeTypeName})");

            // Select random unconnected available doorway for Parent
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();
            Debug.Log($"  Parent room {parentRoom.id} has {unconnectedAvailableParentDoorways.Count} available doorways (total doorways: {parentRoom.doorWayList.Count})");

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                // Log which doorways are unavailable to help diagnose
                int unavailableCount = 0;
                int connectedCount = 0;
                foreach (var dw in parentRoom.doorWayList)
                {
                    if (dw.isUnavailable) unavailableCount++;
                    if (dw.isConnected) connectedCount++;
                }
                Debug.LogWarning($"[CanPlaceRoomWithNoOverlaps] No more available doorways for parent room {parentRoom.id}. Placement FAILED after {placementAttempts} attempts. (connected={connectedCount}, unavailable={unavailableCount})");
                // If no more doorways to try then overlap failure.
                return false; // room overlaps
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];
            Debug.Log($"  Selected parent doorway: orientation={doorwayParent.orientation}, position={doorwayParent.position}");


            // Get a random room template for room node that is consistent with the parent door orientation
            RoomTemplateSO roomtemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            if (roomtemplate == null)
            {
                Debug.LogError($"Không tìm thấy RoomTemplateSO phù hợp cho node: {roomNode.id}, loại: {roomNode.roomNodeType.name}, hướng doorway: {doorwayParent.orientation}");
            }

            string templateInfo = roomtemplate != null ? roomtemplate.guid : "<null>";
            Debug.Log($"  Selected template: {templateInfo}");

            // Create a room
            Room room = CreateRoomFromRoomTemplate(roomtemplate, roomNode);

            // Place the room - returns true if the room doesn't overlap
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // If room doesn't overlap then set to false to exit while loop
                Debug.Log($"  ✓ Room {room.id} placed successfully at lowerBounds={room.lowerBounds}, upperBounds={room.upperBounds}");
                roomOverlaps = false;

                // Mark room as positioned
                room.isPositioned = true;

                // Add room to dictionary
                if (!dungeonBuilderRoomDictionary.ContainsKey(room.id))
                {
                    dungeonBuilderRoomDictionary.Add(room.id, room);
                }
                else
                {
                    Debug.LogWarning($"Room {room.id} already in dictionary when placing");
                }

            }
            else
            {
                Debug.Log($"  ✗ Room placement failed: overlap detected or invalid placement");
                roomOverlaps = true;
            }

        }

        return true;  // no room overlaps
    }

    /// <summary>
    /// Get random room template for room node taking into account the parent doorway orientation.
    /// </summary>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        // If room node is a corridor then select random correct Corridor room template based on
        // parent doorway orientation
        if (roomNode.roomNodeType.isCorridor)
        {

            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;


                case Orientation.east:
                case Orientation.west:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;


                case Orientation.none:
                    break;

                default:
                    break;
            }

        }
        // Else select random room template
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

        }


        return roomtemplate;
    }


    /// <summary>
    /// Place the room - returns true if the room doesn't overlap, false otherwise
    /// </summary>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {

        // Get current room doorway position
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        // Return if no doorway in room opposite to parent doorway
        if (doorway == null)
        {
            Debug.Log($"    [PlaceTheRoom] No opposite doorway found for {doorwayParent.orientation}");
            // Just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }

        // Calculate 'world' grid parent doorway position
        Vector2Int parent = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        // Calculate adjustment position offset based on room doorway position that we are trying to connect (e.g. if this doorway is west then we need to add (1,0) to the east parent doorway)

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        // Calculate room lower bounds and upper bounds based on positioning to align with parent doorway
        room.lowerBounds = parent + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Vector2Int newRoomSize = room.upperBounds - room.lowerBounds;
        Debug.Log($"    [PlaceTheRoom] Calculated placement: room {room.id} (size={newRoomSize}) at lowerBounds={room.lowerBounds}, upperBounds={room.upperBounds}");

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            // mark doorways as connected & unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            // return true to show rooms have been connected with no overlap
            return true;
        }
        else
        {
            Vector2Int overlapRoomSize = overlappingRoom.upperBounds - overlappingRoom.lowerBounds;
            Debug.Log($"    [PlaceTheRoom] OVERLAP detected! New room bounds=({room.lowerBounds}, {room.upperBounds}) size={newRoomSize} overlaps with room {overlappingRoom.id} bounds=({overlappingRoom.lowerBounds}, {overlappingRoom.upperBounds}) size={overlapRoomSize}");
            // Just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }

    }


    /// <summary>
    /// Get the doorway from the doorway list that has the opposite orientation to doorway
    /// </summary>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {

        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }

        return null;

    }


    /// <summary>
    /// Check for rooms that overlap the upper and lower bounds parameters, and if there are overlapping rooms then return room else return null
    /// </summary>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        // Iterate through all rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // skip if same room as room to test or room hasn't been positioned
            if (room.id == roomToTest.id || !room.isPositioned)
                continue;

            // If room overlaps
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }


        // Return
        return null;

    }


    /// <summary>
    /// Check if 2 rooms overlap each other - return true if they overlap or false if they don't overlap
    /// </summary>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);

        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


    /// <summary>
    /// Check if interval 1 overlaps interval 2 - this method is used by the IsOverlappingRoom method
    /// </summary>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Get a random room template from the roomtemplatelist that matches the roomType and return it
    /// (return null if no matching room templates found).
    /// </summary>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // Loop through room template list
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // Add matching room templates
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // Return null if list is zero
        if (matchingRoomTemplateList.Count == 0)
            return null;

        // Select random room template from list and return
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];

    }


    /// <summary>
    /// Get unconnected doorways
    /// </summary>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // Loop through doorway list
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }


    /// <summary>
    /// Create room based on roomTemplate and layoutNode, and return the created room
    /// </summary>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // Initialise room from template
        Room room = new Room();
        if (roomTemplate == null)
        {
            Debug.LogError($"CreateRoomFromRoomTemplate: roomTemplate is null for node {(roomNode!=null?roomNode.id:"<null>")}");
            return null;
        }

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        //room.battleMusic = roomTemplate.battleMusic;
        //room.ambientMusic = roomTemplate.ambientMusic;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        //room.spawnPositionArray = roomTemplate.spawnPositionArray;
        //room.enemiesByLevelList = roomTemplate.enemiesByLevelList;
        //room.roomLevelEnemySpawnParametersList = roomTemplate.roomEnemySpawnParametersList;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray != null ? (Vector2Int[])roomTemplate.spawnPositionArray.Clone() : null;
        room.enemiesByLevelList = CopySpawnableObjectByLevelList(roomTemplate.enemiesByLevelList);
        room.roomLevelEnemySpawnParametersList = roomTemplate.roomEnemySpawnParametersList != null ? new List<RoomEnemySpawnParameters>(roomTemplate.roomEnemySpawnParametersList) : null;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        // Debug info to help diagnose instantiation issues without causing crashes
        string prefabInfo = room.prefab != null ? room.prefab.name : "<null prefab>";
        Debug.Log($"CreateRoomFromRoomTemplate: Created Room object id={room.id}, templateID={room.templateID}, prefab={prefabInfo}, templateLowerBounds={room.templateLowerBounds}, spawnPositions={(room.spawnPositionArray!=null?room.spawnPositionArray.Length.ToString():"null")} ");

        // Set parent ID for room
        // if (roomNode.parentRoomNodeIDList.Count == 0) // Entrance
        // {
        //     room.parentRoomID = "";
        //     room.isPreviouslyVisited = true;

        //     // Set entrance in game manager
        //     //GameManager.Instance.SetCurrentRoom(room);

        // }
        // else
        // {
        //     room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        // }


        // If there are no enemies to spawn then default the room to be clear of enemies
        // if (room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0)
        // {
        //     room.isClearedOfEnemies = true;
        // }


        return room;

    }


    /// <summary>
    /// Select a random room node graph from the list of room node graphs
    /// </summary>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }


    /// <summary>
    /// Create deep copy of doorway list
    /// </summary>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }


    /// <summary>
    /// Create deep copy of string list
    /// </summary>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    /// <summary>
    /// Deep copy spawnable objects by level list
    /// </summary>
    private List<SpawnableObjectsByLevel<EnemyDetailsSO>> CopySpawnableObjectByLevelList(List<SpawnableObjectsByLevel<EnemyDetailsSO>> oldList)
    {
        if (oldList == null) return null;

        var newList = new List<SpawnableObjectsByLevel<EnemyDetailsSO>>();
        foreach (var levelEntry in oldList)
        {
            if (levelEntry == null) continue;
            var copy = new SpawnableObjectsByLevel<EnemyDetailsSO>
            {
                dungeonLevel = levelEntry.dungeonLevel,
                spawnableObjectRatioList = new List<SpawnableObjectRatio<EnemyDetailsSO>>()
            };

            if (levelEntry.spawnableObjectRatioList != null)
            {
                foreach (var ratio in levelEntry.spawnableObjectRatioList)
                {
                    if (ratio == null) continue;
                    copy.spawnableObjectRatioList.Add(new SpawnableObjectRatio<EnemyDetailsSO>
                    {
                        dungeonObject = ratio.dungeonObject,
                        ratio = ratio.ratio
                    });
                }
            }

            newList.Add(copy);
        }

        return newList;
    }

    /// <summary>
    /// Get entrance room if cached
    /// </summary>
    public Room GetEntranceRoom()
    {
        if (entranceRoomCache != null) return entranceRoomCache;

        foreach (var kvp in dungeonBuilderRoomDictionary)
        {
            if (kvp.Value.roomNodeType != null && kvp.Value.roomNodeType.isEntrance)
            {
                entranceRoomCache = kvp.Value;
                break;
            }
        }

        return entranceRoomCache;
    }

    /// <summary>
    /// Instantiate the dungeon room gameobjects from the prefabs
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        // Iterate through all dungeon rooms.
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Calculate room position (remember the room instantiatation position needs to be adjusted by the room template lower bounds)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // Instantiate room
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // Get instantiated room component from instantiated prefab.
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponent<InstantiatedRoom>();

            instantiatedRoom.room = room;

            // Initialise The Instantiated Room
            instantiatedRoom.Initialise(roomGameobject);

            // Save gameobject reference.
            room.instantiatedRoom = instantiatedRoom;

            //// Demo code to set rooms as cleared - except for boss
            //if (!room.roomNodeType.isBossRoom)
            //{
            //    room.isClearedOfEnemies = true;
            //}
        }
    }


    /// <summary>
    /// Get a room template by room template ID, returns null if ID doesn't exist
    /// </summary>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get room by roomID, if no room exists with that ID return null
    /// </summary>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Clear dungeon room gameobjects and dungeon room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        // Destroy instantiated dungeon gameobjects and clear dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictionary.Clear();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Clear dungeon for editor mode. Use DestroyImmediate.
    /// </summary>
    public void ClearDungeonForEditor()
    {
        // Destroy instantiated dungeon gameobjects
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Clear the dictionary
        dungeonBuilderRoomDictionary.Clear();
    }

    /// <summary>
    /// Clear dungeon at runtime. Destroys instantiated dungeon gameobjects and clears dictionary.
    /// </summary>
    public void ClearDungeonRuntime()
    {
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictionary.Clear();
        }

        // Also destroy any remaining children attached to this GameObject
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i) != null)
                Destroy(transform.GetChild(i).gameObject);
        }
    }
#endif
}