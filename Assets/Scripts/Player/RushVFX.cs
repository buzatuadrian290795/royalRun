using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Efecte vizuale post-processing pe durata Rush: Vignette + Chromatic Aberration pulsate
public class RushVFX : MonoBehaviour
{
    [SerializeField] private Volume volume;

    [Header("Vignette")]
    [SerializeField] private float vignetteMin = 0.3f;
    [SerializeField] private float vignetteMax = 0.5f;

    [Header("Chromatic Aberration")]
    [SerializeField] private float chromaticMin = 0.4f;
    [SerializeField] private float chromaticMax = 1f;

    [Header("Timings")]
    [SerializeField] private float pulseSpeed = 3f;  // Cat de repede pulseaza efectele
    [SerializeField] private float fadeSpeed = 4f;   // Cat de repede dispar dupa Rush

    private Vignette m_Vignette;
    private ChromaticAberration m_Chromatic;

    private void Awake()
    {
        if (volume == null) volume = GetComponent<Volume>();

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out m_Vignette);
            volume.profile.TryGet(out m_Chromatic);
        }

        SetIntensities(0f, 0f);
    }

    private void Update()
    {
        bool isActive = RushEffect.Instance != null && RushEffect.Instance.IsActive;

        if (isActive)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1

            if (m_Vignette != null)
                m_Vignette.intensity.value = Mathf.Lerp(vignetteMin, vignetteMax, pulse);

            if (m_Chromatic != null)
                m_Chromatic.intensity.value = Mathf.Lerp(chromaticMin, chromaticMax, pulse);
        }
        else
        {
            if (m_Vignette != null)
                m_Vignette.intensity.value = Mathf.MoveTowards(m_Vignette.intensity.value, 0f, fadeSpeed * Time.deltaTime);

            if (m_Chromatic != null)
                m_Chromatic.intensity.value = Mathf.MoveTowards(m_Chromatic.intensity.value, 0f, fadeSpeed * Time.deltaTime);
        }
    }

    private void SetIntensities(float vignette, float chromatic)
    {
        if (m_Vignette != null) m_Vignette.intensity.value = vignette;
        if (m_Chromatic != null) m_Chromatic.intensity.value = chromatic;
    }
}
