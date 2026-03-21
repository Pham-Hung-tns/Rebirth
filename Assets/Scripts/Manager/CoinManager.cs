using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : Persistence<CoinManager>
{
    public int totalCoins { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
       totalCoins = GameManager.Instance.gameData.totalCoin;
    }
    public void AddCoin(int amount)
    {
        totalCoins += amount;
        UIEvents.OnCoinChanged?.Invoke(totalCoins);
    }

    public void RemoveCoin(int amount) 
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            UIEvents.OnCoinChanged?.Invoke(totalCoins);
        }
    }
}
