using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] float invulnerabilityDuration = 1f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] Renderer[] renderersToBlink;

    RagdollController ragdollController;
    bool isInvulnerable;

    int playerLayer;
    int obstacleLayer;

    float invulnerabilityTimer;
    float blinkTimer;
    bool visible = true;

    private void Awake()
    {
        ragdollController = GetComponent<RagdollController>();

        if (renderersToBlink == null || renderersToBlink.Length == 0)
        {
            renderersToBlink = GetComponentsInChildren<Renderer>(true);
        }

        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void Update()
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= Time.deltaTime;
        blinkTimer -= Time.deltaTime;

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
        if (isInvulnerable) return;
        if (ragdollController == null) return;
        if (ragdollController.IsRagdollActive) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            ragdollController.EnableRagdoll();
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
        foreach (Renderer rend in renderersToBlink)
        {
            if (rend != null)
                rend.enabled = isVisible;
        }
    }
}