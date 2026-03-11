using System.Collections;
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
    Coroutine invulnerabilityRoutine;

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

        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityCoroutine());
    }
    // Schimba coroutina
    private IEnumerator InvulnerabilityCoroutine()
    {
        Debug.Log("Invulnerability START");

        isInvulnerable = true;

        if (playerLayer != -1 && obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, true);
        }

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invulnerabilityDuration)
        {
            visible = !visible;
            SetRenderersVisible(visible);

            Debug.Log("Blink: " + visible);

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetRenderersVisible(true);

        if (playerLayer != -1 && obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(playerLayer, obstacleLayer, false);
        }

        isInvulnerable = false;
        invulnerabilityRoutine = null;

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