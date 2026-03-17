public class Coin : Pickup
{
    protected override void OnPickup()
    {
        int coinsToAdd = m_LevelGenerator != null ? m_LevelGenerator.CoinMultiplier : 1;
        CoinCounterUI.Instance.AddCoin(coinsToAdd);
    }
}