using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private ParticleSystem speedupParticleSystem;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 120f;
    [SerializeField] private float zoomDuration = 1f;
    [SerializeField] private float zoomSpeedModifier = 1f;
    [SerializeField] private float resetFOVDuration = 1f;

    private CinemachineCamera cinemachineCamera;
    private Coroutine fovCoroutine;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        if (speedupParticleSystem != null)
        {
            speedupParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void LateUpdate()
    {
        EnsureTargetIsCurrentPlayer();
    }

    private void EnsureTargetIsCurrentPlayer()
    {
        PlayerRespawnManager respawnManager = FindFirstObjectByType<PlayerRespawnManager>();
        if (respawnManager == null || respawnManager.CurrentPlayer == null || cinemachineCamera == null)
        {
            return;
        }

        Transform currentPlayerTransform = respawnManager.CurrentPlayer.transform;

        if (cinemachineCamera.Target.TrackingTarget != currentPlayerTransform)
        {
            cinemachineCamera.Target.TrackingTarget = currentPlayerTransform;
        }
    }

    public void ChangeCameraFOV(float speedAmount)
    {
        if (cinemachineCamera == null)
            return;

        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }

        fovCoroutine = StartCoroutine(ChangeFOVRoutine(speedAmount));
    }

    public void ResetFOV()
    {
        if (cinemachineCamera == null)
            return;

        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }

        if (speedupParticleSystem != null)
        {
            speedupParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        fovCoroutine = StartCoroutine(ResetFOVRoutine());
    }

    public void SetSpeedupEffectActive(bool isActive)
    {
        if (speedupParticleSystem == null)
            return;

        if (isActive)
        {
            if (!speedupParticleSystem.isPlaying)
            {
                speedupParticleSystem.Play();
            }
        }
        else
        {
            speedupParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private IEnumerator ChangeFOVRoutine(float speedAmount)
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float targetFOV = Mathf.Clamp(startFOV + speedAmount * zoomSpeedModifier, minFOV, maxFOV);

        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            float t = elapsedTime / zoomDuration;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = targetFOV;
        fovCoroutine = null;
    }

    private IEnumerator ResetFOVRoutine()
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < resetFOVDuration)
        {
            float t = elapsedTime / resetFOVDuration;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, minFOV, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = minFOV;
        fovCoroutine = null;
    }
}