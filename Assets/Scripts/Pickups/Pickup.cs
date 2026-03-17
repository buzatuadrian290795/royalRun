using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    private const string PlayerTag = "Player";

    protected LevelGenerator m_LevelGenerator;

    public void Init(LevelGenerator levelGenerator)
    {
        m_LevelGenerator = levelGenerator;
    }

    private void FixedUpdate()
    {
        transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            OnPickup();
            Destroy(gameObject);
        }
    }

    protected abstract void OnPickup();
}