// Pickup care activeaza Rush: viteza maxima + invulnerabilitate pentru durata configurata
public class RushPickup : Pickup
{
    protected override void OnPickup()
    {
        float rushMult = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetMultiplier(UpgradeType.RushDuration)
            : 1f;
        RushEffect.Instance?.Activate(m_Config.rushDuration * rushMult);
    }
}
