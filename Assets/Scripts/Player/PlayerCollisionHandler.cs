using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private RagdollController ragdollController;
    [SerializeField] private float adjustChangeMoveSpeedAmount = 2f;
    
    private int playerLayer;
    private int obstacleLayer;
    private bool isInvulnerable;
    private bool visible = true;
    private float invulnerabilityTimer;
    private float blinkTimer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void FixedUpdate()
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= Time.fixedDeltaTime;
        blinkTimer -= Time.fixedDeltaTime;

        if (blinkTimer <= 0f)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            Debug.Log("Blink: " + visible);

            blinkTimer = blinkInterval;
        }

        if (invulnerabilityTimer <= 0f)
        {
            EndInvulnerability();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.LogError(isInvulnerable);
        if (isInvulnerable) return;
        if (ragdollController == null) return;
        if (ragdollController.IsRagdollActive) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector3 hitPoint = collision.transform.position;
            ragdollController.EnableRagdoll(hitPoint);
        }
    }

    public void StartInvulnerability()
    {
        if (isInvulnerable) return;

        Debug.Log("StartInvulnerability ENTER");
        Debug.Log("Invulnerability START");

        isInvulnerable = true;

        if (playerLayer != -1 && obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);
        }

        invulnerabilityTimer = invulnerabilityDuration;
        blinkTimer = blinkInterval;
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

        Debug.Log("Invulnerability END");
    }

    private void SetRenderersVisible(bool isVisible)
    {
        meshRenderer.enabled = isVisible;
    }
}