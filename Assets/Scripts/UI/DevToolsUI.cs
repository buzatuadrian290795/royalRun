using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Panou Developer Tools: permite modificarea in timp real a variabilelor din LevelGenerator si ChunkConfig
public class DevToolsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelGenerator m_LevelGenerator;
    [SerializeField] private ChunkConfig m_ChunkConfig;

    [Header("Obstacle Density")]
    [SerializeField] private Slider m_ObstacleDensitySlider;      // Range 0 - 1
    [SerializeField] private TMP_Text m_ObstacleDensityValueText;

    [Header("Starting Chunks Amount")]
    [SerializeField] private Slider m_StartingChunksSlider;       // Range 1 - 50
    [SerializeField] private TMP_Text m_StartingChunksValueText;

    [Header("Coin Spawn")]
    [SerializeField] private Slider m_MinCoinsSlider;             // Range 1 - 20
    [SerializeField] private TMP_Text m_MinCoinsValueText;
    [SerializeField] private Slider m_MaxCoinsSlider;             // Range 1 - 20
    [SerializeField] private TMP_Text m_MaxCoinsValueText;

    [Header("Spawn Chances")]
    [SerializeField] private Slider m_AppleSpawnChanceSlider;     // Range 0 - 1
    [SerializeField] private TMP_Text m_AppleSpawnChanceValueText;
    [SerializeField] private Slider m_CoinSpawnChanceSlider;      // Range 0 - 1
    [SerializeField] private TMP_Text m_CoinSpawnChanceValueText;
    [SerializeField] private Slider m_MagnetSpawnChanceSlider;    // Range 0 - 1
    [SerializeField] private TMP_Text m_MagnetSpawnChanceValueText;
    [SerializeField] private Slider m_RushSpawnChanceSlider;      // Range 0 - 1
    [SerializeField] private TMP_Text m_RushSpawnChanceValueText;

    private bool m_IsLoading;

    private void Awake()
    {
        if (m_LevelGenerator == null)
            m_LevelGenerator = FindFirstObjectByType<LevelGenerator>();

        if (m_LevelGenerator == null)
            Debug.LogError("DevToolsUI: LevelGenerator not found.");

        SetupSlider(m_ObstacleDensitySlider, 0f, 1f, false, OnObstacleDensityChanged);
        SetupSlider(m_StartingChunksSlider, 1f, 50f, true, OnStartingChunksChanged);
        SetupSlider(m_MinCoinsSlider, 1f, 20f, true, OnMinCoinsChanged);
        SetupSlider(m_MaxCoinsSlider, 1f, 20f, true, OnMaxCoinsChanged);
        SetupSlider(m_AppleSpawnChanceSlider, 0f, 1f, false, OnAppleSpawnChanceChanged);
        SetupSlider(m_CoinSpawnChanceSlider, 0f, 1f, false, OnCoinSpawnChanceChanged);
        SetupSlider(m_MagnetSpawnChanceSlider, 0f, 1f, false, OnMagnetSpawnChanceChanged);
        SetupSlider(m_RushSpawnChanceSlider, 0f, 1f, false, OnRushSpawnChanceChanged);
    }

    private void SetupSlider(Slider slider, float min, float max, bool wholeNumbers, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.onValueChanged.AddListener(callback);
    }

    private void OnDestroy()
    {
        if (m_ObstacleDensitySlider != null) m_ObstacleDensitySlider.onValueChanged.RemoveListener(OnObstacleDensityChanged);
        if (m_StartingChunksSlider != null) m_StartingChunksSlider.onValueChanged.RemoveListener(OnStartingChunksChanged);
        if (m_MinCoinsSlider != null) m_MinCoinsSlider.onValueChanged.RemoveListener(OnMinCoinsChanged);
        if (m_MaxCoinsSlider != null) m_MaxCoinsSlider.onValueChanged.RemoveListener(OnMaxCoinsChanged);
        if (m_AppleSpawnChanceSlider != null) m_AppleSpawnChanceSlider.onValueChanged.RemoveListener(OnAppleSpawnChanceChanged);
        if (m_CoinSpawnChanceSlider != null) m_CoinSpawnChanceSlider.onValueChanged.RemoveListener(OnCoinSpawnChanceChanged);
        if (m_MagnetSpawnChanceSlider != null) m_MagnetSpawnChanceSlider.onValueChanged.RemoveListener(OnMagnetSpawnChanceChanged);
        if (m_RushSpawnChanceSlider != null) m_RushSpawnChanceSlider.onValueChanged.RemoveListener(OnRushSpawnChanceChanged);
    }

    // Apelat din SettingsUI cand panoul devine vizibil
    public void LoadValues()
    {
        if (m_LevelGenerator == null) return;

        m_IsLoading = true;

        if (m_ObstacleDensitySlider != null)
            m_ObstacleDensitySlider.SetValueWithoutNotify(m_LevelGenerator.ObstacleDensity);
        if (m_StartingChunksSlider != null)
            m_StartingChunksSlider.SetValueWithoutNotify(m_LevelGenerator.StartingChunksAmount);

        UpdateText(m_ObstacleDensityValueText, m_LevelGenerator.ObstacleDensity, "F2");
        UpdateText(m_StartingChunksValueText, m_LevelGenerator.StartingChunksAmount);

        if (m_ChunkConfig != null)
        {
            if (m_MinCoinsSlider != null) m_MinCoinsSlider.SetValueWithoutNotify(m_ChunkConfig.minCoinsToSpawn);
            if (m_MaxCoinsSlider != null) m_MaxCoinsSlider.SetValueWithoutNotify(m_ChunkConfig.maxCoinsToSpawn);
            if (m_AppleSpawnChanceSlider != null) m_AppleSpawnChanceSlider.SetValueWithoutNotify(m_ChunkConfig.appleSpawnChance);
            if (m_CoinSpawnChanceSlider != null) m_CoinSpawnChanceSlider.SetValueWithoutNotify(m_ChunkConfig.coinSpawnChance);

            UpdateText(m_MinCoinsValueText, m_ChunkConfig.minCoinsToSpawn);
            UpdateText(m_MaxCoinsValueText, m_ChunkConfig.maxCoinsToSpawn);
            UpdateText(m_AppleSpawnChanceValueText, m_ChunkConfig.appleSpawnChance, "F2");
            UpdateText(m_CoinSpawnChanceValueText, m_ChunkConfig.coinSpawnChance, "F2");
            if (m_MagnetSpawnChanceSlider != null) m_MagnetSpawnChanceSlider.SetValueWithoutNotify(m_ChunkConfig.magnetSpawnChance);
            UpdateText(m_MagnetSpawnChanceValueText, m_ChunkConfig.magnetSpawnChance, "F2");
            if (m_RushSpawnChanceSlider != null) m_RushSpawnChanceSlider.SetValueWithoutNotify(m_ChunkConfig.rushSpawnChance);
            UpdateText(m_RushSpawnChanceValueText, m_ChunkConfig.rushSpawnChance, "F2");
        }

        m_IsLoading = false;
    }

    // ---------- CALLBACKS ----------

    private void OnObstacleDensityChanged(float value)
    {
        if (m_IsLoading) return;
        m_LevelGenerator?.SetObstacleDensity(value);
        UpdateText(m_ObstacleDensityValueText, value, "F2");
        AudioManager.Instance?.PlayUI();
    }

    private void OnStartingChunksChanged(float value)
    {
        if (m_IsLoading) return;
        int intValue = Mathf.RoundToInt(value);
        m_LevelGenerator?.SetStartingChunksAmount(intValue);
        m_LevelGenerator?.RespawnChunks();
        UpdateText(m_StartingChunksValueText, intValue);
        AudioManager.Instance?.PlayUI();
    }

    private void OnMinCoinsChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        int intValue = Mathf.RoundToInt(value);
        m_ChunkConfig.minCoinsToSpawn = Mathf.Min(intValue, m_ChunkConfig.maxCoinsToSpawn);
        if (m_MinCoinsSlider != null) m_MinCoinsSlider.SetValueWithoutNotify(m_ChunkConfig.minCoinsToSpawn);
        UpdateText(m_MinCoinsValueText, m_ChunkConfig.minCoinsToSpawn);
        AudioManager.Instance?.PlayUI();
    }

    private void OnMaxCoinsChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        int intValue = Mathf.RoundToInt(value);
        m_ChunkConfig.maxCoinsToSpawn = Mathf.Max(intValue, m_ChunkConfig.minCoinsToSpawn);
        if (m_MaxCoinsSlider != null) m_MaxCoinsSlider.SetValueWithoutNotify(m_ChunkConfig.maxCoinsToSpawn);
        UpdateText(m_MaxCoinsValueText, m_ChunkConfig.maxCoinsToSpawn);
        AudioManager.Instance?.PlayUI();
    }

    private void OnAppleSpawnChanceChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        m_ChunkConfig.appleSpawnChance = value;
        UpdateText(m_AppleSpawnChanceValueText, value, "F2");
        AudioManager.Instance?.PlayUI();
    }

    private void OnCoinSpawnChanceChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        m_ChunkConfig.coinSpawnChance = value;
        UpdateText(m_CoinSpawnChanceValueText, value, "F2");
        AudioManager.Instance?.PlayUI();
    }

    private void OnMagnetSpawnChanceChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        m_ChunkConfig.magnetSpawnChance = value;
        UpdateText(m_MagnetSpawnChanceValueText, value, "F2");
        AudioManager.Instance?.PlayUI();
    }

    private void OnRushSpawnChanceChanged(float value)
    {
        if (m_IsLoading || m_ChunkConfig == null) return;
        m_ChunkConfig.rushSpawnChance = value;
        UpdateText(m_RushSpawnChanceValueText, value, "F2");
        AudioManager.Instance?.PlayUI();
    }

    // ---------- TEXT ----------

    private void UpdateText(TMP_Text label, float value, string format = null)
    {
        if (label == null) return;
        label.text = format != null ? value.ToString(format) : value.ToString();
    }

    private void UpdateText(TMP_Text label, int value)
    {
        if (label == null) return;
        label.text = value.ToString();
    }
}
