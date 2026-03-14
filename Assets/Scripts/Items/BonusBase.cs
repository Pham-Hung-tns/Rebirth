using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusBase : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    protected Transform player;

    protected virtual void Update()
    {
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position, player.position) <= 0.1f)
            {
                GetBonus();
                Destroy(gameObject);
            }
        }
        else return;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag))
        {
            player = collision.transform;
        }
    }

    protected virtual void GetBonus()
    {

    }
}
