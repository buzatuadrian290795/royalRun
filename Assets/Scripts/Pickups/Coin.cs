using UnityEngine;

public class Coin : Pickup
{
    private LevelGenerator levelGenerator;

    private void Awake()
    {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();

        if (levelGenerator == null)
        {
            Debug.LogError("Coin: LevelGenerator not found.");
        }
    }

    protected override void OnPickup()
    {
        int coinsToAdd = 1;

        if (levelGenerator != null)
        {
            coinsToAdd = levelGenerator.CoinMultiplier;
        }

        CoinCounterUI.Instance.AddCoin(coinsToAdd);
        Debug.Log("+" + coinsToAdd + " Coin(s)");
    }
}