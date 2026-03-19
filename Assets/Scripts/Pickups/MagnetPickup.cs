using UnityEngine;

// Pickup care activeaza efectul de magnet: atrage monedele din raza spre jucator
public class MagnetPickup : Pickup
{
    protected override void OnPickup()
    {
        if (MagnetEffect.Instance != null && m_Config != null)
            MagnetEffect.Instance.Activate(m_Config);
    }
}
