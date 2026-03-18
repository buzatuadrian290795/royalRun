public class Coin : Pickup
{
    protected override void OnPickup()
    {
        // Adauga monede in functie de multiplicatorul curent de viteza
        // AudioManager.Instance.PlayCoin();   //Trebuie audio nou
        int coinsToAdd = m_LevelGenerator != null ? m_LevelGenerator.CoinMultiplier : 1;
        CoinCounterUI.Instance.AddCoin(coinsToAdd);
    }
}