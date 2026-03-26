using System.Collections;
using UnityEngine;

// Pus pe fiecare prefab de obstacol; la impact in Rush zboara cu Rigidbody si dispare cu VFX
public class ObstacleBreakable : MonoBehaviour
{
    [SerializeField] private float force = 8f;
    [SerializeField] private float upwardForce = 6f;   // Impuls suplimentar in sus la impact
    [SerializeField] private float torque = 4f;
    [SerializeField] private float maxLifetime = 3f;   // Hard cap: distrus dupa maxLifetime indiferent de vizibilitate

    [Header("VFX")]
    [SerializeField] private GameObject breakVFXPrefab;
    [SerializeField] private float vfxLeadTime = 0.3f;

    private Rigidbody m_Rigidbody;
    private Collider m_Collider;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
        }

        m_Collider = GetComponent<Collider>();
    }

    // hitDirection = directia normalizata de la jucator catre obstacol
    public void Break(Vector3 hitDirection)
    {
        // Desprinde de chunk ca sa nu fie distrus de Chunk.Cleanup si sa zboare liber
        transform.SetParent(null);

        // Dezactiveaza coliziunea ca sa nu mai interactioneze cu jucatorul
        if (m_Collider != null) m_Collider.enabled = false;

        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;

        Vector3 flyDir = (hitDirection + Vector3.up * 0.8f).normalized;
        m_Rigidbody.AddForce(flyDir * force, ForceMode.Impulse);
        m_Rigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        m_Rigidbody.AddTorque(Random.onUnitSphere * torque, ForceMode.Impulse);

        StartCoroutine(DestroyWithVFX());
    }

    private IEnumerator DestroyWithVFX()
    {
        yield return new WaitForSeconds(maxLifetime - vfxLeadTime);

        if (breakVFXPrefab != null)
            Instantiate(breakVFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
