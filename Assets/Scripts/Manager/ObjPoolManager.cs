using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool Manager hỗ trợ cả Projectile và Enemy (GameObject chung).
/// - Projectile: pool theo tên prefab (Projectile component).
/// - Enemy/GameObject: pool theo tên prefab (generic).
/// Khi lấy từ pool, gọi IPoolable.OnPoolSpawn() trên tất cả components hỗ trợ.
/// Khi trả về pool, gọi IPoolable.OnPoolDespawn() rồi SetActive(false).
/// </summary>
public class ObjPoolManager : Persistence<ObjPoolManager>
{
    // === PROJECTILE POOL (giữ nguyên API cũ, tương thích code hiện tại) ===
    private readonly Dictionary<string, Queue<Projectile>> projectilePool = new Dictionary<string, Queue<Projectile>>();

    // === GENERIC GAMEOBJECT POOL (dùng cho Enemy và các object khác) ===
    private readonly Dictionary<string, Queue<GameObject>> gameObjectPool = new Dictionary<string, Queue<GameObject>>();

    // Cache IPoolable components để tránh GetComponents mỗi frame
    private readonly Dictionary<GameObject, IPoolable[]> poolableCache = new Dictionary<GameObject, IPoolable[]>();

    protected override void Awake()
    {
        base.Awake();
    }

    #region Projectile Pool (backward compatible)

    /// <summary>
    /// Lấy Projectile từ pool. Nếu pool rỗng, tạo mới.
    /// API giữ nguyên tên "Initialization" để tương thích code cũ.
    /// </summary>
    public Projectile Initialization(Projectile prefab)
    {
        string key = prefab.name;

        if (projectilePool.TryGetValue(key, out Queue<Projectile> queue) && queue.Count > 0)
        {
            Projectile bullet = queue.Dequeue();
            // Đảm bảo object vẫn tồn tại (chưa bị destroy bởi scene change)
            if (bullet != null && bullet.gameObject != null)
                return bullet;
        }

        return CreateNewProjectile(prefab);
    }

    private Projectile CreateNewProjectile(Projectile prefab)
    {
        Projectile newBullet = Instantiate(prefab);
        newBullet.name = prefab.name; // Giữ tên nhất quán cho pooling
        newBullet.gameObject.SetActive(false);
        return newBullet;
    }

    /// <summary>
    /// Trả Projectile về pool.
    /// API giữ nguyên tên "ReturnBullet" để tương thích code cũ.
    /// </summary>
    public void ReturnBullet(Projectile bullet)
    {
        if (bullet == null || bullet.gameObject == null) return;

        bullet.gameObject.SetActive(false);
        // Reparent về pool manager để tránh bị destroy khi room bị clear
        bullet.transform.SetParent(transform);

        string key = bullet.name;
        if (!projectilePool.TryGetValue(key, out Queue<Projectile> queue))
        {
            queue = new Queue<Projectile>();
            projectilePool.Add(key, queue);
        }

        queue.Enqueue(bullet);
    }

    #endregion

    #region Generic GameObject Pool (Enemy, etc.)

    /// <summary>
    /// Lấy GameObject từ pool theo prefab. Nếu pool rỗng, Instantiate mới.
    /// Tự động gọi IPoolable.OnPoolSpawn() trên tất cả components hỗ trợ.
    /// </summary>
    /// <param name="prefab">Prefab gốc để tạo instance.</param>
    /// <param name="position">Vị trí world spawn.</param>
    /// <param name="rotation">Rotation khi spawn.</param>
    /// <param name="parent">Transform cha (thường là room transform).</param>
    /// <returns>GameObject sẵn sàng sử dụng (đã SetActive(false), caller tự active).</returns>
    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        string key = prefab.name;
        GameObject obj = null;

        if (gameObjectPool.TryGetValue(key, out Queue<GameObject> queue))
        {
            // Tìm object hợp lệ trong pool (loại bỏ những object đã bị destroy)
            while (queue.Count > 0)
            {
                obj = queue.Dequeue();
                if (obj != null) break;
                obj = null;
            }
        }

        if (obj != null)
        {
            // Tái sử dụng từ pool
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            // Gọi OnPoolSpawn trên tất cả IPoolable components
            NotifyPoolSpawn(obj);
        }
        else
        {
            // Pool rỗng → Instantiate mới
            obj = Instantiate(prefab, position, rotation, parent);
            obj.name = key; // Giữ tên nhất quán (không có "(Clone)")
        }

        return obj;
    }

    /// <summary>
    /// Trả GameObject về pool. Gọi IPoolable.OnPoolDespawn() rồi SetActive(false).
    /// Object được reparent về ObjPoolManager để tránh bị destroy theo room.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        // Gọi OnPoolDespawn trên tất cả IPoolable components
        NotifyPoolDespawn(obj);

        obj.SetActive(false);
        obj.transform.SetParent(transform); // Reparent để tránh bị destroy khi room clear

        string key = obj.name;
        if (!gameObjectPool.TryGetValue(key, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            gameObjectPool.Add(key, queue);
        }

        queue.Enqueue(obj);
    }

    /// <summary>
    /// Pre-warm pool bằng cách tạo sẵn một số lượng instances.
    /// Gọi khi load level để tránh GC spike trong gameplay.
    /// </summary>
    /// <param name="prefab">Prefab gốc.</param>
    /// <param name="count">Số lượng cần tạo sẵn.</param>
    public void PreWarm(GameObject prefab, int count)
    {
        string key = prefab.name;
        if (!gameObjectPool.ContainsKey(key))
        {
            gameObjectPool[key] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            obj.name = key;
            obj.SetActive(false);
            gameObjectPool[key].Enqueue(obj);
        }
    }

    #endregion

    #region IPoolable Notification Helpers

    private void NotifyPoolSpawn(GameObject obj)
    {
        IPoolable[] poolables = GetCachedPoolables(obj);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnPoolSpawn();
        }
    }

    private void NotifyPoolDespawn(GameObject obj)
    {
        IPoolable[] poolables = GetCachedPoolables(obj);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnPoolDespawn();
        }
    }

    private IPoolable[] GetCachedPoolables(GameObject obj)
    {
        if (!poolableCache.TryGetValue(obj, out IPoolable[] poolables))
        {
            poolables = obj.GetComponents<IPoolable>();
            poolableCache[obj] = poolables;
        }
        return poolables;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Xóa toàn bộ pool (Projectile + GameObject). Gọi khi chuyển scene/kết thúc game.
    /// </summary>
    public void ClearAllPool()
    {
        // Clear projectile pool
        foreach (var queue in projectilePool.Values)
        {
            foreach (var projectile in queue)
            {
                if (projectile != null && projectile.gameObject != null)
                {
                    Destroy(projectile.gameObject);
                }
            }
            queue.Clear();
        }
        projectilePool.Clear();

        // Clear generic GameObject pool
        foreach (var queue in gameObjectPool.Values)
        {
            foreach (var obj in queue)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            queue.Clear();
        }
        gameObjectPool.Clear();

        // Clear cache
        poolableCache.Clear();
    }

    /// <summary>
    /// Xóa pool của một prefab cụ thể theo tên.
    /// </summary>
    public void ClearPool(string prefabName)
    {
        if (gameObjectPool.TryGetValue(prefabName, out Queue<GameObject> queue))
        {
            foreach (var obj in queue)
            {
                if (obj != null) Destroy(obj);
            }
            queue.Clear();
        }
    }

    /// <summary>
    /// Trả về số lượng object đang sẵn sàng trong pool cho một prefab.
    /// </summary>
    public int GetPoolCount(string prefabName)
    {
        if (gameObjectPool.TryGetValue(prefabName, out Queue<GameObject> queue))
            return queue.Count;
        return 0;
    }

    #endregion
}
