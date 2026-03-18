using UnityEngine;

// Clasa de baza pentru toate obiectele colectabile (mar, moneda etc.)
public abstract class Pickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    private const string PlayerTag = "Player";

    protected LevelGenerator m_LevelGenerator;

    // Injectat de Chunk la instantiere
    public void Init(LevelGenerator levelGenerator)
    {
        m_LevelGenerator = levelGenerator;
    }

    private void FixedUpdate()
    {
        transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f); // Rotatie continua pe Y
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            OnPickup();   // Comportament specific fiecarei subclase
            Destroy(gameObject);
        }
    }

    // Implementat de fiecare subclasa (Apple, Coin etc.)
    protected abstract void OnPickup();
}