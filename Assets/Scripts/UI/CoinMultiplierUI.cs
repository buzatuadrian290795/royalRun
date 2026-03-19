using TMPro;
using UnityEngine;

// Afiseaza in timp real multiplicatorul de monede din LevelGenerator
public class CoinMultiplierUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private LevelGenerator levelGenerator;

    private int m_LastMultiplier = -1;

    private void Awake()
    {
        // Cauta automat LevelGenerator in scena daca nu e setat din Inspector
        if (levelGenerator == null)
            levelGenerator = FindFirstObjectByType<LevelGenerator>();

        if (multiplierText == null)
            Debug.LogError("CoinMultiplierUI: multiplierText is not set.");
        if (levelGenerator == null)
            Debug.LogError("CoinMultiplierUI: LevelGenerator not found.");
    }

    private void Update()
    {
        if (multiplierText == null || levelGenerator == null) return;

        int multiplier = levelGenerator.CoinMultiplier;
        if (multiplier == m_LastMultiplier) return;

        m_LastMultiplier = multiplier;
        multiplierText.text = "x" + multiplier;
    }
}