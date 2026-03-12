using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody mainRigidbody;
    [SerializeField] Collider mainCollider;
    [SerializeField] PlayerView playerMovementScript;
    [SerializeField] PlayerCollisionHandler collisionHandler;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] Transform spawnPoint;

    Rigidbody[] ragdollRigidbodies;
    Collider[] ragdollColliders;
    Transform cachedTransform;

    bool isRagdollActive;
    bool respawnScheduled;
    float respawnTimer;

    public bool IsRagdollActive => isRagdollActive;

    private void Awake()
    {
        cachedTransform = transform;

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

        if (spawnPoint != null)
            cachedTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        DisableRagdollImmediate();
    }

    private void FixedUpdate()
    {
        if (!respawnScheduled)
            return;

        respawnTimer -= Time.deltaTime;

        if (respawnTimer > 0f)
            return;

        respawnTimer = 0f;
        respawnScheduled = false;
        Respawn();
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

    public void EnableRagdoll()
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

            mainRigidbody.useGravity = false;
            mainRigidbody.isKinematic = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
        }

        SetRagdollState(true);
    }

    private void Respawn()
    {
        SetRagdollState(false);

        if (spawnPoint != null)
        {
            cachedTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            Physics.SyncTransforms();
        }

        if (mainRigidbody != null)
        {
            mainRigidbody.useGravity = false;

            if (mainRigidbody.isKinematic)
                mainRigidbody.isKinematic = false;
            
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            
            mainRigidbody.position = cachedTransform.position;
            mainRigidbody.rotation = cachedTransform.rotation;
            mainRigidbody.Sleep();
        }

        if (mainCollider != null)
            mainCollider.enabled = true;

        if (mainRigidbody != null)
            mainRigidbody.isKinematic = false;

        if (animator != null)
            animator.enabled = true;

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

            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //rb.isKinematic = !enabled;
            //rb.useGravity = enabled;
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