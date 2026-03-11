using System.Collections;
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

    Coroutine respawnRoutine;
    bool isRagdollActive;

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

        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        respawnRoutine = StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // 1. opreste ragdoll total
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb == mainRigidbody) continue;

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null || col == mainCollider) continue;
            col.enabled = false;
        }

        // 2. muta playerul exact la spawn
        yield return null;
        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        yield return null;
        Physics.SyncTransforms();

        // 3. reseteaza rigidbody-ul principal
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = false;
            mainRigidbody.useGravity = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            mainRigidbody.isKinematic = true;
            mainRigidbody.position = transform.position;
            mainRigidbody.rotation = transform.rotation;
            mainRigidbody.Sleep();
        }

        // 4. reporneste corpul principal
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
        respawnRoutine = null;
    }

    private void DisableRagdollImmediate()
    {
        isRagdollActive = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == null || rb == mainRigidbody) continue;

            rb.isKinematic = false;
            rb.useGravity = false;
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