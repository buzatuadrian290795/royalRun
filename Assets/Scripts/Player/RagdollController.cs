using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody mainRigidbody;
    [SerializeField] Collider mainCollider;
    [SerializeField] PlayerController playerMovementScript;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] Transform spawnPoint;

    Rigidbody[] ragdollRigidbodies;
    Collider[] ragdollColliders;

    bool isRagdollActive;
    bool respawnScheduled;
    float respawnTimer;

    public bool IsRagdollActive => isRagdollActive;

    private void Awake()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        DisableRagdollImmediate();
    }

    private void Update()
    {
        if (!respawnScheduled) return;

        respawnTimer -= Time.deltaTime;

        if (respawnTimer <= 0f)
        {
            respawnScheduled = false;
            Respawn();
        }
    }

    public void EnableRagdoll()
    {
        if (isRagdollActive) return;

        isRagdollActive = true;

        if (animator != null)
            animator.enabled = false;

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        if (mainRigidbody != null)
        {
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            mainRigidbody.isKinematic = true;
            mainRigidbody.useGravity = false;
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb == mainRigidbody) continue;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null || col == mainCollider) continue;
            col.enabled = true;
        }

        respawnTimer = respawnDelay;
        respawnScheduled = true;
    }

    private void Respawn()
    {
        // 1. oprește ragdoll total
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb == mainRigidbody) continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null || col == mainCollider) continue;
            col.enabled = false;
        }

        // 2. mută playerul exact la spawn
        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            Physics.SyncTransforms();
        }

        // 3. resetează rigidbody-ul principal
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = false;
            mainRigidbody.useGravity = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            mainRigidbody.position = transform.position;
            mainRigidbody.rotation = transform.rotation;
            mainRigidbody.isKinematic = true;
            mainRigidbody.Sleep();
        }

        // 4. repornește corpul principal
        if (mainCollider != null)
            mainCollider.enabled = true;

        if (mainRigidbody != null)
            mainRigidbody.isKinematic = false;

        if (animator != null)
            animator.enabled = true;

        PlayerCollisionHandler collisionHandler = GetComponent<PlayerCollisionHandler>();
        if (collisionHandler != null)
            collisionHandler.StartInvulnerability();

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        isRagdollActive = false;
    }

    private void DisableRagdollImmediate()
    {
        isRagdollActive = false;
        respawnScheduled = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb == mainRigidbody) continue;

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null || col == mainCollider) continue;
            col.enabled = false;
        }

        if (mainCollider != null)
            mainCollider.enabled = true;

        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = false;
            mainRigidbody.useGravity = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
        }

        if (animator != null)
            animator.enabled = true;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
    }
}