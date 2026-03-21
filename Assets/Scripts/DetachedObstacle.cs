using UnityEngine;

// Distruge obstacolul detasat in Rush cand iese din campul vizual al camerei
public class DetachedObstacle : MonoBehaviour
{
    private Camera m_Camera;
    private Renderer m_Renderer;
    private readonly Plane[] m_FrustumPlanes = new Plane[6];

    private void Awake()
    {
        m_Camera = Camera.main;
        m_Renderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (m_Camera == null) return;

        GeometryUtility.CalculateFrustumPlanes(m_Camera, m_FrustumPlanes);
        Bounds bounds = m_Renderer != null ? m_Renderer.bounds : new Bounds(transform.position, Vector3.one);

        if (!GeometryUtility.TestPlanesAABB(m_FrustumPlanes, bounds))
            Destroy(gameObject);
    }
}
