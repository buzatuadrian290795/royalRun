using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// Gestioneaza FOV-ul camerei si efectul de particule la accelerare
public class CameraController : MonoBehaviour
{
    [SerializeField] private ParticleSystem speedupParticleSystem;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 100f;
    [SerializeField] private float zoomDuration = 1f;
    [SerializeField] private float zoomSpeedModifier = 1f; // Cat de mult afecteaza viteza FOV-ul
    [SerializeField] private float resetFOVDuration = 1f;

    private CinemachineCamera cinemachineCamera;
    private Coroutine fovCoroutine; // Referinta la coroutine activa (pentru a o putea intrerupe)
    private PlayerRespawnManager m_RespawnManager;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        speedupParticleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Start()
    {
        m_RespawnManager = FindFirstObjectByType<PlayerRespawnManager>();
    }

    private void LateUpdate()
    {
        EnsureTargetIsCurrentPlayer();
    }

    // Actualizeaza tinta camerei dupa fiecare respawn (daca jucatorul s-a schimbat)
    private void EnsureTargetIsCurrentPlayer()
    {
        if (m_RespawnManager == null || m_RespawnManager.CurrentPlayer == null || cinemachineCamera == null)
            return;

        Transform currentPlayerTransform = m_RespawnManager.CurrentPlayer.transform;
        if (cinemachineCamera.Target.TrackingTarget != currentPlayerTransform)
            cinemachineCamera.Target.TrackingTarget = currentPlayerTransform;
    }

    // Mareste/micsoreaza FOV proportional cu speedAmount si porneste particulele
    public void ChangeCameraFOV(float speedAmount)
    {
        if (cinemachineCamera == null) return;
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(ChangeFOVRoutine(speedAmount));
        if (speedAmount > 0) speedupParticleSystem.Play();
    }

    // Revine la FOV minim si opreste particulele
    public void ResetFOV()
    {
        if (cinemachineCamera == null) return;
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        speedupParticleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fovCoroutine = StartCoroutine(ResetFOVRoutine());
    }

    // Porneste sau opreste efectul de particule (apelat din exterior)
    public void SetSpeedupEffectActive(bool isActive)
    {
        if (speedupParticleSystem == null) return;
        if (isActive)
        {
            if (!speedupParticleSystem.isPlaying) speedupParticleSystem.Play();
        }
        else
        {
            speedupParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Interpoleaza FOV de la valoarea curenta la cea tinta (clamped intre min si max)
    private IEnumerator ChangeFOVRoutine(float speedAmount)
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float targetFOV = Mathf.Clamp(startFOV + speedAmount * zoomSpeedModifier, minFOV, maxFOV);
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = targetFOV;
        fovCoroutine = null;
    }

    // Interpoleaza FOV inapoi la minFOV
    private IEnumerator ResetFOVRoutine()
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < resetFOVDuration)
        {
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, minFOV, elapsed / resetFOVDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = minFOV;
        fovCoroutine = null;
    }
}