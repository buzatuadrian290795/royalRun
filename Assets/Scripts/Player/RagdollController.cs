using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody mainRigidbody;
    [SerializeField] Collider mainCollider;
    [SerializeField] PlayerView playerMovementScript;
    [SerializeField] PlayerCollisionHandler collisionHandler;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] float knockbackUpwardForce = 2f;

    Rigidbody[] ragdollRigidbodies;
    Collider[] ragdollColliders;

    bool isRagdollActive;
    bool respawnScheduled;
    float respawnTimer;

    public bool IsRagdollActive => isRagdollActive;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (mainRigidbody == null)
            mainRigidbody = GetComponent<Rigidbody>();

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();

        if (playerMovementScript == null)
            playerMovementScript = GetComponent<PlayerView>();

        if (collisionHandler == null)
            collisionHandler = GetComponent<PlayerCollisionHandler>();

        CacheRagdollParts();
        DisableRagdollImmediate();
    }

    private void FixedUpdate()
    {
        if (!respawnScheduled)
            return;

        respawnTimer -= Time.fixedDeltaTime;

        if (respawnTimer > 0f)
            return;

        respawnScheduled = false;

        PlayerRespawnManager respawnManager = FindFirstObjectByType<PlayerRespawnManager>();

        if (respawnManager != null)
        {
            respawnManager.SpawnPlayer();
        }
        else
        {
            Debug.LogError("RagdollController: PlayerRespawnManager not found in scene.");
        }

        Destroy(gameObject);
    }

    private void CacheRagdollParts()
    {
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        int ragdollRbCount = 0;
        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i] != null && allRigidbodies[i] != mainRigidbody)
                ragdollRbCount++;
        }

        ragdollRigidbodies = new Rigidbody[ragdollRbCount];
        int rbIndex = 0;

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            Rigidbody rb = allRigidbodies[i];
            if (rb != null && rb != mainRigidbody)
                ragdollRigidbodies[rbIndex++] = rb;
        }

        int ragdollColCount = 0;
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i] != null && allColliders[i] != mainCollider)
                ragdollColCount++;
        }

        ragdollColliders = new Collider[ragdollColCount];
        int colIndex = 0;

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col != null && col != mainCollider)
                ragdollColliders[colIndex++] = col;
        }
    }

    public void EnableRagdoll(Vector3 hitSourcePosition)
    {
        if (isRagdollActive)
            return;

        isRagdollActive = true;
        respawnScheduled = true;
        respawnTimer = respawnDelay;

        if (animator != null)
            animator.enabled = false;

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        if (mainRigidbody != null)
        {
            mainRigidbody.useGravity = true;
            mainRigidbody.isKinematic = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
        }

        SetRagdollState(true);
        ApplyKnockback(hitSourcePosition);
    }

    private void ApplyKnockback(Vector3 hitSourcePosition)
    {
        Vector3 direction = (transform.position - hitSourcePosition).normalized;
        direction.y = 0f;

        Vector3 force = direction * knockbackForce + Vector3.up * knockbackUpwardForce;

        for (int i = 0; i < ragdollRigidbodies.Length; i++)
        {
            Rigidbody rb = ragdollRigidbodies[i];
            if (rb == null)
                continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    private void DisableRagdollImmediate()
    {
        isRagdollActive = false;
        respawnScheduled = false;
        respawnTimer = 0f;

        SetRagdollState(false);

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

    private void SetRagdollState(bool enabled)
    {
        for (int i = 0; i < ragdollRigidbodies.Length; i++)
        {
            Rigidbody rb = ragdollRigidbodies[i];

            if (rb == null)
            {
                Debug.LogError($"RagdollController: ragdollRigidbodies[{i}] is NULL");
                continue;
            }

            if (enabled)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        for (int i = 0; i < ragdollColliders.Length; i++)
        {
            if (ragdollColliders[i] == null)
            {
                Debug.LogError($"RagdollController: ragdollColliders[{i}] is NULL");
                continue;
            }

            ragdollColliders[i].enabled = enabled;
        }
    }
}