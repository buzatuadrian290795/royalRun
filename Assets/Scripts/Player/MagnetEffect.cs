using System.Collections.Generic;
using UnityEngine;

// Componenta pusa pe Player; atrage monedele din raza spre jucator pe durata efectului
public class MagnetEffect : MonoBehaviour
{
    public static MagnetEffect Instance { get; private set; }

    public bool IsActive => m_TimeRemaining > 0f;
    public float TimeRemaining => m_TimeRemaining;

    private float m_TimeRemaining;
    private float m_AttractSpeed;
    private float m_AttractRadius;

    // Monedele detasate din chunk de catre magnet
    private readonly List<Coin> m_DetachedCoins = new List<Coin>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Activate(ChunkConfig config)
    {
        m_TimeRemaining  = config.magnetDuration;
        m_AttractSpeed   = config.magnetAttractSpeed;
        m_AttractRadius  = config.magnetAttractRadius;
    }

    public void Deactivate()
    {
        ReattachDetachedCoins();
        m_TimeRemaining = 0f;
    }

    private void Update()
    {
        if (m_TimeRemaining <= 0f) return;

        m_TimeRemaining -= Time.deltaTime;

        if (m_TimeRemaining <= 0f)
        {
            ReattachDetachedCoins();
            return;
        }

        float radiusSqr = m_AttractRadius * m_AttractRadius;

        foreach (Coin coin in Coin.All)
        {
            if ((coin.transform.position - transform.position).sqrMagnitude > radiusSqr) continue;

            if (coin.transform.parent != null)
            {
                coin.transform.SetParent(null);
                m_DetachedCoins.Add(coin);
            }

            coin.transform.position = Vector3.MoveTowards(
                coin.transform.position,
                transform.position,
                m_AttractSpeed * Time.deltaTime
            );
        }
    }

    private void ReattachDetachedCoins()
    {
        foreach (Coin coin in m_DetachedCoins)
        {
            if (coin == null) continue;

            Transform nearestChunk = FindNearestChunk(coin.transform.position);
            if (nearestChunk != null)
                coin.transform.SetParent(nearestChunk);
        }

        m_DetachedCoins.Clear();
    }

    private Transform FindNearestChunk(Vector3 position)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (Chunk chunk in Chunk.All)
        {
            float dist = Mathf.Abs(chunk.transform.position.z - position.z);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = chunk.transform;
            }
        }

        return best;
    }
}
