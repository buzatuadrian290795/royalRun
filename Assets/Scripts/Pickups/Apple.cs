using UnityEngine;

// Clasa pentru colectabilul "Mar", mosteneste comportamentul de baza din Pickup
public class Apple : Pickup
{
    // Cat de mult se modifica viteza chunk-urilor la colectare
    [SerializeField] private float adjustChangeMoveSpeedAmount = 2f;

    // Suprascrie metoda abstracta din Pickup, apelata automat la colectare
    protected override void OnPickup()
    {
        VibrationManager.Instance.Vibrate(50);                              // Vibreaza 50ms
        AudioManager.Instance.PlayApple();                                  // Reda sunetul marului
        m_LevelGenerator?.ChangeChunkMoveSpeed(adjustChangeMoveSpeedAmount); // Creste viteza (daca LevelGenerator exista)
    }
}