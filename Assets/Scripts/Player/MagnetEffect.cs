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
        float magnetMult = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetMultiplier(UpgradeType.MagnetPower)
            : 1f;
        m_TimeRemaining  = config.magnetDuration    * magnetMult;
        m_AttractSpeed   = config.magnetAttractSpeed * magnetMult;
        m_AttractRadius  = config.magnetAttractRadius * magnetMult;
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
        Vector3 myPos   = transform.position;
        float   step    = m_AttractSpeed * Time.deltaTime;

        for (int i = Coin.All.Count - 1; i >= 0; i--)
        {
            Coin coin = Coin.All[i];
            if (coin == null) continue;

            Vector3 coinPos = coin.transform.position;
            if ((coinPos - myPos).sqrMagnitude > radiusSqr) continue;

            if (coin.transform.parent != null)
            {
                coin.transform.SetParent(null);
                m_DetachedCoins.Add(coin);
            }

            coin.transform.position = Vector3.MoveTowards(coinPos, myPos, step);
        }
    }

    private void ReattachDetachedCoins()
    {
        for (int i = 0; i < m_DetachedCoins.Count; i++)
        {
            Coin coin = m_DetachedCoins[i];
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

        for (int i = 0; i < Chunk.All.Count; i++)
        {
            float dist = Mathf.Abs(Chunk.All[i].transform.position.z - position.z);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = Chunk.All[i].transform;
            }
        }

        return best;
    }
}
