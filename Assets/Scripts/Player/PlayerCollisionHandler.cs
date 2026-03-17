using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private PlayerView playerView;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private RagdollController ragdollController;

    private int playerLayer;
    private int obstacleLayer;
    private bool isInvulnerable;
    private bool visible = true;
    private float invulnerabilityTimer;
    private float blinkTimer;

    private void Awake()
    {
        if (playerView == null)
        {
            playerView = GetComponent<PlayerView>();
        }

        if (playerView == null)
        {
            Debug.LogError("PlayerCollisionHandler: PlayerView not found.");
        }

        if (meshRenderer == null)
        {
            Debug.LogError("PlayerCollisionHandler: Mesh Renderer not set.");
        }

        if (ragdollController == null)
        {
            Debug.LogError("PlayerCollisionHandler: RagdollController not set.");
        }

        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void FixedUpdate()
    {
        if (!isInvulnerable)
        {
            return;
        }

        invulnerabilityTimer -= Time.fixedDeltaTime;
        blinkTimer -= Time.fixedDeltaTime;

        if (blinkTimer <= 0f)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            blinkTimer = playerView.BlinkInterval;
        }

        if (invulnerabilityTimer <= 0f)
        {
            EndInvulnerability();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isInvulnerable)
        {
            return;
        }

        if (ragdollController == null)
        {
            return;
        }

        if (ragdollController.IsRagdollActive)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector3 hitPoint = collision.transform.position;
            ragdollController.EnableRagdoll(hitPoint);
        }
    }

    public void StartInvulnerability()
    {
        Debug.Log("Invulnerability START");
        Debug.Log("Duration = " + playerView.InvulnerabilityDuration);
        Debug.Log("BlinkInterval = " + playerView.BlinkInterval);

        if (isInvulnerable)
        {
            return;
        }

        if (playerView == null)
        {
            Debug.LogError("PlayerCollisionHandler: Cannot start invulnerability because PlayerView is missing.");
            return;
        }

        isInvulnerable = true;

        if (playerLayer != -1 && obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);
        }

        invulnerabilityTimer = playerView.InvulnerabilityDuration;
        blinkTimer = playerView.BlinkInterval;
        visible = true;
        SetRenderersVisible(true);
    }

    private void EndInvulnerability()
    {
        SetRenderersVisible(true);

        if (playerLayer != -1 && obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);
        }

        isInvulnerable = false;
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = isVisible;
        }
    }
}