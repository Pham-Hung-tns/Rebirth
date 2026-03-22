using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed;
    [Tooltip("Thời gian sống tối đa (giây). Tự trả về pool nếu không va chạm gì.")]
    private float projectileLifetime = 5f;
    public float Speed { get; set; }
    public Vector3 Direction { get; set; }

    private GameObject owner;
    private int damage;
    private Vector2 knockbackDir;
    private float knockbackForce;
    private GameObject ownerRoot;
    private float aliveTimer;

    // Start is called before the first frame update
    void Start()
    {
        Speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Direction * (speed * Time.deltaTime), Space.World);

        // Auto-return khi hết lifetime (tránh bay mãi ngoài màn hình)
        aliveTimer += Time.deltaTime;
        if (aliveTimer >= projectileLifetime)
        {
            ReturnBullet();
        }
    }

    /// <summary>
    /// Khởi tạo projectile. Gọi mỗi lần bắn (cả lần đầu và khi tái sử dụng từ pool).
    /// </summary>
    /// <param name="lifetime">Thời gian sống (giây). Truyền từ RangeWeaponDataSO.projectileLifetime.
    /// Nếu <= 0, dùng giá trị mặc định trên prefab.</param>
    public void Initialize(GameObject owner, float speed, int damage, Vector2 knockbackDir, float knockbackForce, float lifetime = -1f)
    {
        this.owner = owner;
        // store root of owner hierarchy to ignore collisions with any child collider
        if (owner != null)
            ownerRoot = owner.transform.root.gameObject;
        else
            ownerRoot = null;
        this.speed = speed;
        this.damage = damage;
        this.knockbackDir = knockbackDir;
        this.knockbackForce = knockbackForce;

        // Cập nhật lifetime từ SO (nếu được truyền)
        if (lifetime > 0f)
            projectileLifetime = lifetime;

        // Reset lifetime timer khi tái sử dụng từ pool
        aliveTimer = 0f;

        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collision with owner or any child of owner's hierarchy
        if (ownerRoot != null)
        {
            if (collision.transform == ownerRoot.transform || collision.transform.IsChildOf(ownerRoot.transform))
                return;
        }
        else
        {
            if (collision.gameObject == owner) return;
        }

        ITakeDamage td = collision.GetComponent<ITakeDamage>();
        if(td != null)
        {
            td.TakeDamage(damage, owner, knockbackDir, knockbackForce);
            ReturnBullet();
            return;
        }

        if (collision.CompareTag(Settings.COLLISION_TILEMAP_TAG) || collision.CompareTag(Settings.FRONT_TILEMAP_TAG))
        {
            ReturnBullet();
            return;
        }
    }

    private void ReturnBullet()
    {
        if (ObjPoolManager.Instance != null)
        {
            ObjPoolManager.Instance.ReturnBullet(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
