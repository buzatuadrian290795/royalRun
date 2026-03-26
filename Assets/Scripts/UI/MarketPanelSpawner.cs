using UnityEngine;

// Instantiaza dinamic cate un buton de upgrade per tip, in containerul din Market Panel
public class MarketPanelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform container;

    private void OnEnable()
    {
        Spawn();
    }

    private void Spawn()
    {
        if (buttonPrefab == null)  { Debug.LogWarning("MarketPanelSpawner: buttonPrefab nu e asignat."); return; }
        if (container == null)     { Debug.LogWarning("MarketPanelSpawner: container nu e asignat."); return; }
        if (UpgradeManager.Instance == null) { Debug.LogWarning("MarketPanelSpawner: UpgradeManager.Instance e null."); return; }

        // Sterge butoanele vechi
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        // Instantiaza cate un buton per upgrade definit in UpgradeManager
        foreach (var def in UpgradeManager.Instance.Upgrades)
        {
            if (def == null) continue;
            GameObject go = Instantiate(buttonPrefab, container);
            var btn = go.GetComponent<MarketUpgradeButtonUI>();
            if (btn != null) btn.Init(def.type);
        }
    }
}
