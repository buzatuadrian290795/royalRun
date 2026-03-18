using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Panou Developer Tools: permite modificarea in timp real a variabilelor din LevelGenerator
public class DevToolsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelGenerator m_LevelGenerator;

    [Header("Obstacle Density")]
    [SerializeField] private Slider m_ObstacleDensitySlider;      // Range 0 - 1
    [SerializeField] private TMP_Text m_ObstacleDensityValueText; // Afiseaza valoarea curenta

    [Header("Starting Chunks Amount")]
    [SerializeField] private Slider m_StartingChunksSlider;       // Range 1 - 30
    [SerializeField] private TMP_Text m_StartingChunksValueText;  // Afiseaza valoarea curenta

    // Flag activ cat timp incarcam valorile in UI (previne aplicarea accidentala)
    private bool m_IsLoading;

    private void Awake()
    {
        if (m_LevelGenerator == null)
            m_LevelGenerator = FindFirstObjectByType<LevelGenerator>();

        if (m_LevelGenerator == null)
            Debug.LogError("DevToolsUI: LevelGenerator not found.");

        if (m_ObstacleDensitySlider != null)
        {
            m_ObstacleDensitySlider.minValue = 0f;
            m_ObstacleDensitySlider.maxValue = 1f;
            m_ObstacleDensitySlider.onValueChanged.AddListener(OnObstacleDensityChanged);
        }

        if (m_StartingChunksSlider != null)
        {
            m_StartingChunksSlider.minValue = 1f;
            m_StartingChunksSlider.maxValue = 30f;
            m_StartingChunksSlider.wholeNumbers = true;
            m_StartingChunksSlider.onValueChanged.AddListener(OnStartingChunksChanged);
        }
    }

    private void OnDestroy()
    {
        if (m_ObstacleDensitySlider != null)
            m_ObstacleDensitySlider.onValueChanged.RemoveListener(OnObstacleDensityChanged);
        if (m_StartingChunksSlider != null)
            m_StartingChunksSlider.onValueChanged.RemoveListener(OnStartingChunksChanged);
    }

    // Apelat din SettingsUI cand panoul devine vizibil; incarca valorile curente din LevelGenerator
    public void LoadValues()
    {
        if (m_LevelGenerator == null) return;

        m_IsLoading = true;

        if (m_ObstacleDensitySlider != null)
            m_ObstacleDensitySlider.SetValueWithoutNotify(m_LevelGenerator.ObstacleDensity);

        if (m_StartingChunksSlider != null)
            m_StartingChunksSlider.SetValueWithoutNotify(m_LevelGenerator.StartingChunksAmount);

        UpdateObstacleDensityText(m_LevelGenerator.ObstacleDensity);
        UpdateStartingChunksText(m_LevelGenerator.StartingChunksAmount);

        m_IsLoading = false;
    }

    // ---------- CALLBACKS ----------

    private void OnObstacleDensityChanged(float value)
    {
        if (m_IsLoading) return;
        m_LevelGenerator?.SetObstacleDensity(value);
        UpdateObstacleDensityText(value);
        AudioManager.Instance?.PlayUI();
    }

    private void OnStartingChunksChanged(float value)
    {
        if (m_IsLoading) return;
        int intValue = Mathf.RoundToInt(value);
        m_LevelGenerator?.SetStartingChunksAmount(intValue);
        UpdateStartingChunksText(intValue);
        AudioManager.Instance?.PlayUI();
    }

    // ---------- TEXT ----------

    private void UpdateObstacleDensityText(float value)
    {
        if (m_ObstacleDensityValueText != null)
            m_ObstacleDensityValueText.text = value.ToString("F2");
    }

    private void UpdateStartingChunksText(int value)
    {
        if (m_StartingChunksValueText != null)
            m_StartingChunksValueText.text = value.ToString();
    }

    private void UpdateStartingChunksText(float value) =>
        UpdateStartingChunksText(Mathf.RoundToInt(value));
}
