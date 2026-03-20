using UnityEngine;

// Adaugat automat pe obstacol cand e detasat in Rush; il re-ataseaza la chunk cand se opreste
public class DetachedObstacle : MonoBehaviour
{
    [SerializeField] private float sleepVelocityThreshold = 0.05f;
    [SerializeField] private float maxDetachedTime = 5f;

    private Rigidbody m_Rigidbody;
    private bool m_RushEnded;
    private float m_DetachedTimer;

    public void Init(Rigidbody rb)
    {
        m_Rigidbody = rb;
    }

    private void Update()
    {
        // Asteapta sa se termine Rush-ul
        if (!m_RushEnded)
        {
            if (RushEffect.Instance != null && RushEffect.Instance.IsActive) return;
            m_RushEnded = true;
        }

        // Timerul porneste doar dupa ce Rush s-a terminat
        m_DetachedTimer += Time.deltaTime;

        if (m_DetachedTimer >= maxDetachedTime)
        {
            Destroy(gameObject);
            return;
        }

        // Rush s-a terminat → asteapta ca obstacolul sa se opreasca
        if (m_Rigidbody == null || IsStill())
            Reattach();
    }

    private bool IsStill()
    {
        float threshold = sleepVelocityThreshold * sleepVelocityThreshold;
        return m_Rigidbody.linearVelocity.sqrMagnitude  < threshold &&
               m_Rigidbody.angularVelocity.sqrMagnitude < threshold;
    }

    private void Reattach()
    {
        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity  = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic     = true;
        }

        Transform bestChunk = FindNearestChunk();

        if (bestChunk == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(bestChunk);
        Destroy(this);
    }

    private Transform FindNearestChunk()
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (Chunk chunk in Chunk.All)
        {
            float dist = Mathf.Abs(chunk.transform.position.z - transform.position.z);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = chunk.transform;
            }
        }

        return best;
    }
}
