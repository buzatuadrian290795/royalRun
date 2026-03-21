using UnityEngine;

// Gestioneaza coliziunile jucatorului si perioada de invulnerabilitate dupa impact
public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private PlayerView playerView;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private RagdollController ragdollController;
    [SerializeField] private float rushHitForce = 5f;
    [SerializeField] private float rushHitForceY = 0f;
    [SerializeField] private float rushHitForceZ = 0f;
    private MagnetEffect magnetEffect;
    private CameraController cameraController;

    private int playerLayer;
    private int obstacleLayer;

    private bool isInvulnerable;
    private bool visible = true;
    private float invulnerabilityTimer; // Cat timp mai dureaza invulnerabilitatea
    private float blinkTimer;           // Cat timp pana la urmatoarea clipire

    private void Awake()
    {
        if (playerView == null) playerView = GetComponent<PlayerView>();
        magnetEffect = GetComponent<MagnetEffect>();
        cameraController = FindFirstObjectByType<CameraController>();

        if (playerView == null) Debug.LogError("PlayerCollisionHandler: PlayerView not found.");
        if (meshRenderer == null) Debug.LogError("PlayerCollisionHandler: Mesh Renderer not set.");
        if (ragdollController == null) Debug.LogError("PlayerCollisionHandler: RagdollController not set.");

        // Cache-uieste layerele o singura data (mai eficient decat LayerMask in Update)
        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void FixedUpdate()
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= Time.fixedDeltaTime;
        blinkTimer -= Time.fixedDeltaTime;

        // Alterneaza vizibilitatea la fiecare BlinkInterval secunde
        if (blinkTimer <= 0f)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            blinkTimer = playerView.BlinkInterval;
        }

        if (invulnerabilityTimer <= 0f)
            EndInvulnerability();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignora coliziunile daca e invulnerabil sau ragdoll-ul e deja activ
        if (isInvulnerable || ragdollController == null || ragdollController.IsRagdollActive)
            return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (RushEffect.Instance != null && RushEffect.Instance.IsActive)
            {
                GameObject obstacle = collision.gameObject;
                obstacle.transform.SetParent(null);

                Rigidbody rb = obstacle.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;

                Vector3 dir = (obstacle.transform.position - transform.position).normalized;
                dir.y = Mathf.Abs(dir.y) + 0.3f;
                rb.AddForce(dir * rushHitForce + new Vector3(0f, rushHitForceY, -rushHitForceZ), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * rushHitForce, ForceMode.Impulse);

                DetachedObstacle detached = obstacle.GetComponent<DetachedObstacle>();
                detached.enabled = true;
                RushEffect.AddObstacleHit();
                cameraController?.Shake();
                return;
            }

            VibrationManager.Instance.Vibrate(100);
            AudioManager.Instance.PlayAuch();
            ragdollController.EnableRagdoll(collision.transform.position);
            if (magnetEffect != null) magnetEffect.Deactivate();
        }
    }

    // Porneste perioada de invulnerabilitate (apelata de ex. dupa respawn)
    public void StartInvulnerability()
    {
        if (isInvulnerable) return;
        if (playerView == null)
        {
            Debug.LogError("PlayerCollisionHandler: Cannot start invulnerability because PlayerView is missing.");
            return;
        }

        isInvulnerable = true;

        // Dezactiveaza coliziunile intre Player si Obstacle pe durata invulnerabilitatii
        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);

        invulnerabilityTimer = playerView.InvulnerabilityDuration;
        blinkTimer = playerView.BlinkInterval;
        visible = true;
        SetRenderersVisible(true);
    }

    // Termina invulnerabilitatea: face jucatorul vizibil si reactiveaza coliziunile
    private void EndInvulnerability()
    {
        SetRenderersVisible(true);
        if (playerLayer != -1 && obstacleLayer != -1)
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);
        isInvulnerable = false;
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (meshRenderer != null) meshRenderer.enabled = isVisible;
    }
}