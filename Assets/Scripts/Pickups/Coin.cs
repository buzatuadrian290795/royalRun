public class Coin : Pickup
{
    protected override void OnPickup()
    {
        //AudioManager.Instance.PlayCoin();
        int coinsToAdd = m_LevelGenerator != null ? m_LevelGenerator.CoinMultiplier : 1;
        CoinCounterUI.Instance.AddCoin(coinsToAdd);
    }
}