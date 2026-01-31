using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed;
    public float Speed { get; set; }
    public Vector3 Direction { get; set; }

    private GameObject owner;
    private int damage;
    private Vector2 knockbackDir;
    private float knockbackForce;
    private GameObject ownerRoot;

    // Start is called before the first frame update
    void Start()
    {
        Speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Direction * (speed *Time.deltaTime), Space.World);
    }

    public void Initialize(GameObject owner, float speed, int damage, Vector2 knockbackDir, float knockbackForce)
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
        gameObject.SetActive(true);
        Debug.Log($"[Projectile] Initialized. Owner={ownerRoot?.name} Speed={speed} Damage={damage}");
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

        if (collision.CompareTag(Settings.collisionTilemapTag))
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
