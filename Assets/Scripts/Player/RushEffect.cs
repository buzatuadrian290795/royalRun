using UnityEngine;

// Componenta pusa pe Player; activeaza viteza maxima si invulnerabilitate pe durata efectului
public class RushEffect : MonoBehaviour
{
    public static RushEffect Instance { get; private set; }

    public bool IsActive => m_TimeRemaining > 0f || m_GraceRemaining > 0f;
    public bool IsInGrace => m_TimeRemaining <= 0f && m_GraceRemaining > 0f;
    public float TimeRemaining => m_TimeRemaining;

    // +1 la multiplicator pentru fiecare 5 obstacole lovite in Rush; se reseteaza la moarte
    private static int m_ObstacleHitCount;
    public static int ObstacleHitBonus => m_ObstacleHitCount / 5;
    public static void AddObstacleHit() => m_ObstacleHitCount++;
    public static void ResetObstacleBonus() => m_ObstacleHitCount = 0;

    [SerializeField] private float speedAfterRush = 20f;
    [SerializeField] private float gracePeriod = 2f;

    private float m_TimeRemaining;
    private float m_GraceRemaining;
    private bool m_IsActive;
    private LevelGenerator m_LevelGenerator;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        EndRush();
    }

    public void Activate(float duration)
    {
        m_TimeRemaining = duration;
        m_GraceRemaining = 0f;
        m_IsActive = true;

        if (m_LevelGenerator == null)
            m_LevelGenerator = FindFirstObjectByType<LevelGenerator>();
        m_LevelGenerator?.SetMoveSpeedToMax();
    }

    public void Deactivate()
    {
        m_TimeRemaining = 0f;
        m_GraceRemaining = 0f;
        EndRush();
    }

    private void Update()
    {
        if (!m_IsActive) return;

        if (m_TimeRemaining > 0f)
        {
            m_TimeRemaining -= Time.deltaTime;

            // Timerul principal s-a terminat → intra in grace period
            if (m_TimeRemaining <= 0f)
            {
                m_TimeRemaining = 0f;
                m_GraceRemaining = gracePeriod;
                m_LevelGenerator?.SetMoveSpeed(speedAfterRush);
            }
        }
        else if (m_GraceRemaining > 0f)
        {
            m_GraceRemaining -= Time.deltaTime;

            if (m_GraceRemaining <= 0f)
                EndRush();
        }
    }

    private void EndRush()
    {
        if (!m_IsActive) return;
        m_IsActive = false;
        m_LevelGenerator?.SetMoveSpeed(speedAfterRush);
    }
}
