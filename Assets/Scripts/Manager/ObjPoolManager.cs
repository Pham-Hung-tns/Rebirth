using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPoolManager : Persistance<ObjPoolManager>
{
    Dictionary<string, Queue<Projectile>> pool = new Dictionary<string, Queue<Projectile>>();
    protected override void Awake()
    {
        base.Awake();
    }

    public Projectile Initialization(Projectile obj)
    {
        if(pool.TryGetValue(obj.name, out Queue<Projectile> queue))
        {
            if(queue.Count == 0)
            {
                return CreateNewBullet(obj);
            }
            else
            {
                Projectile newBullet = queue.Dequeue();
                return newBullet;
            }
        }
        else
            return CreateNewBullet(obj);
    }

    private Projectile CreateNewBullet( Projectile obj)
    {
        Projectile newBullet = Instantiate(obj);
        newBullet.name = obj.name;
        newBullet.gameObject.SetActive(false);
        return newBullet;
    }

    public void ReturnBullet(Projectile obj)
    {
        if(pool.TryGetValue(obj.name, out Queue<Projectile> queue))
        {
            queue.Enqueue(obj);
        }
        else
        {
            Queue<Projectile> newQueue = new Queue<Projectile>();
            newQueue.Enqueue(obj);
            pool.Add(obj.name, newQueue);
        }
        obj.gameObject.SetActive(false);
    }

    public void ClearAllPool()
    {
        foreach (var queue in pool.Values)
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
        pool.Clear();
    }
}
