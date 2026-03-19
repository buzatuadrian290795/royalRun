using UnityEngine;

// Clasa de baza pentru toate obiectele colectabile (mar, moneda etc.)
public abstract class Pickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    private const string PlayerTag = "Player";

    protected LevelGenerator m_LevelGenerator;
    protected ChunkConfig m_Config;
    protected GameObject m_Player;

    // Injectat de Chunk la instantiere
    public void Init(LevelGenerator levelGenerator, ChunkConfig config)
    {
        m_LevelGenerator = levelGenerator;
        m_Config = config;
    }

    private void FixedUpdate()
    {
        transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f); // Rotatie continua pe Y
    }

    private void OnTriggerEnter(Collider other) => TryPickup(other);
    private void OnTriggerStay(Collider other) => TryPickup(other);

    private void TryPickup(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        if (!CanPickup()) return;

        m_Player = other.gameObject;
        OnPickup();   // Comportament specific fiecarei subclase
        Destroy(gameObject);
    }

    // Suprascris de subclase pentru a bloca pickup-ul in anumite conditii
    protected virtual bool CanPickup() => true;

    // Implementat de fiecare subclasa (Apple, Coin etc.)
    protected abstract void OnPickup();
}