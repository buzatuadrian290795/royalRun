using UnityEngine;

// Container de date pentru jucator: expune componentele si parametrii necesari altor scripturi
public class PlayerView : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public Rigidbody RigidBody { get; private set; }

    [field: SerializeField] public float InvulnerabilityDuration { get; private set; } = 1f;
    [field: SerializeField] public float BlinkInterval { get; private set; } = 0.1f;

    private void Awake()
    {
        if (Animator == null) Debug.LogError("PlayerView: Animator not set.");
        if (RigidBody == null) Debug.LogError("PlayerView: Rigidbody not set.");
    }
}