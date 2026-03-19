// Pickup care activeaza Rush: viteza maxima + invulnerabilitate pentru durata configurata
public class RushPickup : Pickup
{
    protected override void OnPickup()
    {
        RushEffect.Instance?.Activate(m_Config.rushDuration);
    }
}
