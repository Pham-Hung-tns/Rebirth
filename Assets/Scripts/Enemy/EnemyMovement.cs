using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Simple A* pathfinding implementation using Tilemaps (CanMove / Wall)
// Lightweight and suitable for small room-sized graphs on mobile.
public class EnemyMovement : MonoBehaviour
{
    private Tilemap groundTilemap; // CanMove
    private Tilemap collisionTilemap; // Wall (optional)

    private List<Vector3> currentPath = null;
    private int currentPathIndex = 0;
    private float lastPathRequestTime = -999f;
    private EnemyController brain;
    private bool isChasing = false; // Flag để biết khi nào ChaseAction đang active
    private float lastDirectionChangeTime = -999f; // Timer để delay gọi ChangeDirection
    private float directionChangeInterval = 0.3f; // Delay giữa các lần ChangeDirection (0.2s)
    
    // Room grid cache (allocated when spawned in a room)
    private bool hasRoomGrid = false;
    private int gridWidth = 0;
    private int gridHeight = 0;
    private int gridSize = 0;
    private Vector2Int templateLower;
    private bool[] walkable;
    private int[] gScoreArr;
    private int[] fScoreArr;
    private int[] parentArr;
    private byte[] closedArr;
    private int[] heap; // binary heap of indices
    private int heapCount = 0;

    public Tilemap GroundTilemap { get => groundTilemap; set => groundTilemap = value; }
    public Tilemap CollisionTilemap { get => collisionTilemap; set => collisionTilemap = value; }

    public void SetChasing(bool chasing) => isChasing = chasing;

    public void Initialized(EnemyController controller)
    {
        brain = controller;
    }

    private void Start()
    {
        // If spawned as part of a Room, prefer the room's tilemaps (InstantiatedRoom sets these at spawn)
        var room = GetComponentInParent<InstantiatedRoom>();
        if (room != null)
        {
            if (GroundTilemap == null && room.groundTilemap != null)
                GroundTilemap = room.groundTilemap;
            if (CollisionTilemap == null && room.collisionTilemap != null)
                CollisionTilemap = room.collisionTilemap;

            // Build grid cache based on room template bounds (fast lookup for A*)
            if (GroundTilemap != null && room.room != null)
            {
                templateLower = room.room.templateLowerBounds;
                Vector2Int templateUpper = room.room.templateUpperBounds;
                gridWidth = templateUpper.x - templateLower.x + 1;
                gridHeight = templateUpper.y - templateLower.y + 1;
                gridSize = gridWidth * gridHeight;
                walkable = new bool[gridSize];
                gScoreArr = new int[gridSize];
                fScoreArr = new int[gridSize];
                parentArr = new int[gridSize];
                closedArr = new byte[gridSize];
                heap = new int[gridSize + 4];

                // Precompute walkable flags
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        int wx = x + templateLower.x;
                        int wy = y + templateLower.y;
                        var cell = new Vector3Int(wx, wy, 0);
                        bool can = GroundTilemap.HasTile(cell);
                        if (can && CollisionTilemap != null && CollisionTilemap.HasTile(cell))
                            can = false;
                        walkable[x + y * gridWidth] = can;
                    }
                }

                hasRoomGrid = true;
            }
        }
        else
        {
            // If not spawned in a room, do not auto-find by name (we rely on InstantiatedRoom assignment).
            if (GroundTilemap == null)
                Debug.LogWarning($"EnemyMovement on '{gameObject.name}' has no groundTilemap assigned from InstantiatedRoom.");
        }
    }

    private void Update()
    {
        // follow path if available
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            MoveAlongPathStep();
        }
        else if (isChasing)
        {
            // Nếu đang chase mà không có path, di chuyển trực tiếp tới player (tránh khựng lại)
            MoveTowardPatrolPosition();
        }
    }

    // Public: request a path to world target (throttled by Settings.enemyPathRebuildCooldown)
    public void RequestPath(Vector3 worldTarget)
    {
        if (Time.time - lastPathRequestTime < Settings.enemyPathRebuildCooldown)
            return;
        lastPathRequestTime = Time.time;

        var path = FindPath(transform.position, worldTarget);
        if (path != null && path.Count > 0)
        {
            currentPath = path;
            currentPathIndex = 0;
        }
    }

    // Move one step along the current path using Rigidbody2D.MovePosition
    private void MoveAlongPathStep()
    {
        if (brain == null || brain.Rb == null || brain.EnemyConfig == null)
            return;
        Vector2 rbPos = brain.Rb.position;
        Vector3 target = currentPath[currentPathIndex];
        float step = brain.EnemyConfig.speed * Time.deltaTime;
        Vector2 next = Vector2.MoveTowards(rbPos, (Vector2)target, step);
        brain.ChangeDirection(target);
        brain.Rb.MovePosition(next);

        if (Vector2.Distance(next, (Vector2)target) < 0.05f)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                currentPath = null; // reached
            }
        }
    }

    // Di chuyển trực tiếp tới PatrolPosition khi không có path
    private void MoveTowardPatrolPosition()
    {
        if (brain == null || brain.Rb == null || brain.EnemyConfig == null)
            return;
        
        Vector2 rbPos = brain.Rb.position;
        Vector2 targetPos = brain.PatrolPosition;
        float step = brain.EnemyConfig.speed * Time.deltaTime;
        Vector2 next = Vector2.MoveTowards(rbPos, targetPos, step);
        
        // Chỉ gọi ChangeDirection với delay để tránh xoay quá nhanh
        if (Time.time - lastDirectionChangeTime >= directionChangeInterval)
        {
            brain.ChangeDirection(targetPos);
            lastDirectionChangeTime = Time.time;
        }
        
        brain.Rb.MovePosition(next);
    }

    // Convert world to cell and run A* on tile grid
    public List<Vector3> FindPath(Vector3 worldStart, Vector3 worldTarget)
    {
        if (GroundTilemap == null)
        {
            Debug.LogWarning("EnemyMovement: groundTilemap not assigned (no InstantiatedRoom). Pathfinding aborted.");
            return null;
        }

        Vector3Int startCell = GroundTilemap.WorldToCell(worldStart);
        Vector3Int targetCell = GroundTilemap.WorldToCell(worldTarget);

        // quick bail
        if (startCell == targetCell)
            return new List<Vector3> { GroundTilemap.GetCellCenterWorld(targetCell) };

        // If we have a precomputed room grid, use the optimized array-based A*
        if (hasRoomGrid)
        {
            return FindPathUsingGrid(startCell, targetCell);
        }

        // Fallback to simpler A* if no room grid
        return FindPathFallback(startCell, targetCell);
    }

    // Fallback original A* (keeps previous behavior)
    private List<Vector3> FindPathFallback(Vector3Int startCell, Vector3Int targetCell)
    {
        var openSet = new List<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int>();
        var fScore = new Dictionary<Vector3Int, int>();

        openSet.Add(startCell);
        gScore[startCell] = 0;
        fScore[startCell] = Heuristic(startCell, targetCell);

        int iterations = 0;
        int maxIterations = 5000; // safety limit for big rooms

        while (openSet.Count > 0 && iterations++ < maxIterations)
        {
            // get lowest f
            Vector3Int current = openSet[0];
            int bestF = fScore.ContainsKey(current) ? fScore[current] : int.MaxValue;
            for (int i = 1; i < openSet.Count; i++)
            {
                var cell = openSet[i];
                int f = fScore.ContainsKey(cell) ? fScore[cell] : int.MaxValue;
                if (f < bestF)
                {
                    bestF = f;
                    current = cell;
                }
            }

            if (current == targetCell)
            {
                return ReconstructPathFallback(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var n in GetNeighbors(current))
            {
                int tentativeG = gScore.ContainsKey(current) ? gScore[current] + 1 : int.MaxValue;
                if (!gScore.ContainsKey(n) || tentativeG < gScore[n])
                {
                    cameFrom[n] = current;
                    gScore[n] = tentativeG;
                    fScore[n] = tentativeG + Heuristic(n, targetCell);
                    if (!openSet.Contains(n))
                        openSet.Add(n);
                }
            }
        }

        return null;
    }

    private List<Vector3> ReconstructPathFallback(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var total = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            total.Add(current);
        }
        total.Reverse();
        var worldPath = new List<Vector3>(total.Count);
        foreach (var c in total)
            worldPath.Add(GroundTilemap.GetCellCenterWorld(c));
        return worldPath;
    }

    // Optimized grid-based A* using preallocated arrays and a simple binary heap
    private List<Vector3> FindPathUsingGrid(Vector3Int startCell, Vector3Int targetCell)
    {
        int sx = startCell.x - templateLower.x;
        int sy = startCell.y - templateLower.y;
        int tx = targetCell.x - templateLower.x;
        int ty = targetCell.y - templateLower.y;
        if (sx < 0 || sx >= gridWidth || sy < 0 || sy >= gridHeight) return null;
        if (tx < 0 || tx >= gridWidth || ty < 0 || ty >= gridHeight) return null;

        int startIdx = sx + sy * gridWidth;
        int targetIdx = tx + ty * gridWidth;

        // init arrays
        for (int i = 0; i < gridSize; i++)
        {
            gScoreArr[i] = int.MaxValue;
            fScoreArr[i] = int.MaxValue;
            parentArr[i] = -1;
            closedArr[i] = 0;
        }
        heapCount = 0;

        gScoreArr[startIdx] = 0;
        fScoreArr[startIdx] = HeuristicIndex(startIdx, targetIdx);
        HeapPush(startIdx);

        while (heapCount > 0)
        {
            int current = HeapPop();
            if (closedArr[current] == 1) continue; // stale
            if (current == targetIdx)
            {
                // reconstruct
                var rev = new List<int>();
                int cur = current;
                while (cur != -1)
                {
                    rev.Add(cur);
                    cur = parentArr[cur];
                }
                rev.Reverse();
                var path = new List<Vector3>(rev.Count);
                foreach (var idx in rev)
                {
                    int x = (idx % gridWidth) + templateLower.x;
                    int y = (idx / gridWidth) + templateLower.y;
                    path.Add(GroundTilemap.GetCellCenterWorld(new Vector3Int(x, y, 0)));
                }
                return path;
            }

            closedArr[current] = 1;

            int cx = (current % gridWidth);
            int cy = (current / gridWidth);

            // neighbors: 4-directional
            int[,] dirs = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
            for (int di = 0; di < 4; di++)
            {
                int nx = cx + dirs[di, 0];
                int ny = cy + dirs[di, 1];
                if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight) continue;
                int nIdx = nx + ny * gridWidth;
                if (!walkable[nIdx]) continue;
                if (closedArr[nIdx] == 1) continue;

                int tentativeG = gScoreArr[current] + 1;
                if (tentativeG < gScoreArr[nIdx])
                {
                    parentArr[nIdx] = current;
                    gScoreArr[nIdx] = tentativeG;
                    fScoreArr[nIdx] = tentativeG + HeuristicIndex(nIdx, targetIdx);
                    HeapPush(nIdx);
                }
            }
        }

        return null; // no path
    }

    private int HeuristicIndex(int idxA, int idxB)
    {
        int ax = (idxA % gridWidth) + templateLower.x;
        int ay = (idxA / gridWidth) + templateLower.y;
        int bx = (idxB % gridWidth) + templateLower.x;
        int by = (idxB / gridWidth) + templateLower.y;
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    private void HeapPush(int idx)
    {
        int i = heapCount++;
        heap[i] = idx;
        // bubble up
        while (i > 0)
        {
            int parent = (i - 1) >> 1;
            if (fScoreArr[heap[parent]] <= fScoreArr[heap[i]]) break;
            int tmp = heap[parent]; heap[parent] = heap[i]; heap[i] = tmp;
            i = parent;
        }
    }

    private int HeapPop()
    {
        int ret = heap[0];
        heapCount--;
        heap[0] = heap[heapCount];
        int i = 0;
        while (true)
        {
            int left = i * 2 + 1;
            int right = left + 1;
            int smallest = i;
            if (left < heapCount && fScoreArr[heap[left]] < fScoreArr[heap[smallest]]) smallest = left;
            if (right < heapCount && fScoreArr[heap[right]] < fScoreArr[heap[smallest]]) smallest = right;
            if (smallest == i) break;
            int tmp = heap[i]; heap[i] = heap[smallest]; heap[smallest] = tmp;
            i = smallest;
        }
        return ret;
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var total = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            total.Add(current);
        }
        total.Reverse();
        var worldPath = new List<Vector3>(total.Count);
        foreach (var c in total)
            worldPath.Add(GroundTilemap.GetCellCenterWorld(c));
        return worldPath;
    }

    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        // Favor X distance for side-scrolling behavior
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        // 4-directional neighbors
        Vector3Int[] dirs = new Vector3Int[] {
            new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0), new Vector3Int(0,-1,0)
        };
        foreach (var d in dirs)
        {
            var n = cell + d;
            if (IsWalkable(n)) yield return n;
        }
    }

    private bool IsWalkable(Vector3Int cell)
    {
        // walkable if groundTilemap has tile AND collision tilemap does not have tile
        bool hasGround = GroundTilemap.HasTile(cell);
        if (!hasGround) return false;
        if (CollisionTilemap != null && CollisionTilemap.HasTile(cell)) return false;
        return true;
    }
}
