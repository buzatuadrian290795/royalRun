using UnityEngine;

public class Coin : Pickup
{
    protected override void OnPickup()
    {
        CoinCounterUI.Instance.AddCoin(1);
        Debug.Log("+1 Coin");
    }
}
