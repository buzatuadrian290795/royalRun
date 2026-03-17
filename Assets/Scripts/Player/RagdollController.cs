using Unity.Cinemachine;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody mainRigidbody;
    [SerializeField] Collider mainCollider;
    [SerializeField] PlayerView playerMovementScript;
    [SerializeField] PlayerCollisionHandler collisionHandler;
    [SerializeField] CameraController cameraController;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] float knockbackUpwardForce = 2f;

    [SerializeField] float screenShakeForce = 2f;

    private CinemachineImpulseSource m_ImpulseSource;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private PlayerRespawnManager m_RespawnManager;

    private bool isRagdollActive;
    private bool respawnScheduled;
    private float respawnTimer;

    public bool IsRagdollActive => isRagdollActive;

    public void Init(PlayerRespawnManager respawnManager)
    {
        m_RespawnManager = respawnManager;
    }

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
        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraController>();

        m_ImpulseSource = GetComponent<CinemachineImpulseSource>();

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

        if (m_RespawnManager != null)
            m_RespawnManager.SpawnPlayer();
        else
            Debug.LogError("RagdollController: RespawnManager not injected — apeleaza Init().");

        Destroy(gameObject);
    }

    private void CacheRagdollParts()
    {
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        int rbCount = 0;
        for (int i = 0; i < allRigidbodies.Length; i++)
            if (allRigidbodies[i] != mainRigidbody) rbCount++;

        ragdollRigidbodies = new Rigidbody[rbCount];
        int rbIndex = 0;
        for (int i = 0; i < allRigidbodies.Length; i++)
            if (allRigidbodies[i] != mainRigidbody)
                ragdollRigidbodies[rbIndex++] = allRigidbodies[i];

        int colCount = 0;
        for (int i = 0; i < allColliders.Length; i++)
            if (allColliders[i] != mainCollider) colCount++;

        ragdollColliders = new Collider[colCount];
        int colIndex = 0;
        for (int i = 0; i < allColliders.Length; i++)
            if (allColliders[i] != mainCollider)
                ragdollColliders[colIndex++] = allColliders[i];
    }

    public void EnableRagdoll(Vector3 hitSourcePosition)
    {
        if (isRagdollActive) return;

        isRagdollActive = true;
        respawnScheduled = true;
        respawnTimer = respawnDelay;

        if (m_ImpulseSource != null)
            m_ImpulseSource.GenerateImpulse(screenShakeForce);

        cameraController?.ResetFOV();

        if (animator != null) animator.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;

        if (mainRigidbody != null)
        {
            mainRigidbody.useGravity = false;
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
            if (rb == null) continue;
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

        if (mainCollider != null) mainCollider.enabled = true;

        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = false;
            mainRigidbody.useGravity = false;
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
        }

        if (animator != null) animator.enabled = true;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void SetRagdollState(bool enabled)
    {
        for (int i = 0; i < ragdollRigidbodies.Length; i++)
        {
            Rigidbody rb = ragdollRigidbodies[i];
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
        }

        for (int i = 0; i < ragdollColliders.Length; i++)
        {
            if (ragdollColliders[i] != null)
                ragdollColliders[i].enabled = enabled;
        }
    }
}