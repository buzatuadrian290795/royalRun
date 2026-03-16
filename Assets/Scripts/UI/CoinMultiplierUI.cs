using TMPro;
using UnityEngine;

public class CoinMultiplierUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private LevelGenerator levelGenerator;

    private void Awake()
    {
        if (levelGenerator == null)
        {
            levelGenerator = FindFirstObjectByType<LevelGenerator>();
        }

        if (multiplierText == null)
        {
            Debug.LogError("CoinMultiplierUI: multiplierText is not set.");
        }

        if (levelGenerator == null)
        {
            Debug.LogError("CoinMultiplierUI: LevelGenerator not found.");
        }
    }

    private void Update()
    {
        if (multiplierText == null || levelGenerator == null)
        {
            return;
        }

        multiplierText.text = "x" + levelGenerator.CoinMultiplier;
    }
}