using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [field: SerializeField] 
    public Animator Animator { get; private set; }

    [field: SerializeField] 
    public Rigidbody RigidBody { get; private set; }

    // [field: SerializeField]
    // public SkinnedMeshRenderer SkinnedMeshRenderer { get; private set; }

    [field: SerializeField] 
    public float InvulnerabilityDuration { get; private set; } = 1f;

    [field: SerializeField] 
    public float BlinkInterval { get; private set; } = 0.1f;

    private void Awake()
    {
        if (Animator == null)
        {
            Debug.LogError("PlayerController: Animator not set.");
        }

        if (RigidBody == null)
        {
            Debug.LogError("RigidBody: Rigidbody not set.");
        }

        // if (SkinnedMeshRenderer == null)
        // {
        //     Debug.LogError("SkinnedMeshRenderer: Rigidbody not set.");
        // }
    }
}