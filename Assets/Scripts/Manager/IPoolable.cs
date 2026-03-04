/// <summary>
/// Interface cho các object có thể tái sử dụng qua Object Pool.
/// Implement interface này trên MonoBehaviour gắn vào prefab được pool.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Được gọi khi object được lấy ra từ pool (thay thế Awake/Start cho lần tái sử dụng).
    /// Reset toàn bộ state về trạng thái ban đầu tại đây.
    /// </summary>
    void OnPoolSpawn();

    /// <summary>
    /// Được gọi khi object được trả về pool (trước khi SetActive(false)).
    /// Dọn dẹp state, stop coroutines, unsubscribe events tại đây.
    /// </summary>
    void OnPoolDespawn();
}
