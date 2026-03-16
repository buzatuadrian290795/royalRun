using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] ParticleSystem speedupParticleSystem;
    [SerializeField] float minFOV = 60f;
    [SerializeField] float maxFOV = 120f;
    [SerializeField] float zoomDuration = 1f;
    [SerializeField] float zoomSpeedModifier = 1f;
    [SerializeField] float resetFOVDuration = 1f;

    CinemachineCamera cinemachineCamera;
    Coroutine fovCoroutine;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
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

        if (speedAmount > 0)
        {
            speedupParticleSystem.Play();
        }
    }

    public void ResetFOV()
    {
        if (cinemachineCamera == null)
            return;

        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }

        fovCoroutine = StartCoroutine(ResetFOVRoutine());
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