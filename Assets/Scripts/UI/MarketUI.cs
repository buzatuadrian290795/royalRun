using UnityEngine;
using UnityEngine.UI;

// Gestioneaza Market Button si Market Panel: deschide/inchide panoul de market
public class MarketUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject marketPanel;

    [Header("Buttons")]
    [SerializeField] Button marketButton;
    [SerializeField] Button closeButton;

    [Header("Elemente ascunse la deschidere")]
    [SerializeField] GameObject shopButton;
    [SerializeField] GameObject xButton;
    [SerializeField] GameObject distanceObject;
    [SerializeField] GameObject coinsObject;
    [SerializeField] GameObject rushObject;
    [SerializeField] GameObject magnetObject;

    private GameObject[] hiddenTargets;
    private bool[] previousStates;

    private void Awake()
    {
        if (marketPanel == null) Debug.LogWarning("MarketUI: marketPanel is not set.");
        if (marketButton == null) Debug.LogWarning("MarketUI: marketButton is not set.");

        if (marketButton != null) marketButton.onClick.AddListener(OpenMarket);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMarket);

        hiddenTargets = new GameObject[] { shopButton, xButton, distanceObject, coinsObject };
        previousStates = new bool[hiddenTargets.Length];
    }

    private void Start()
    {
        if (marketPanel != null) marketPanel.SetActive(false);
        if (marketButton != null) marketButton.gameObject.SetActive(true);
    }

    public void OpenMarket()
    {
        if (MenuUI.IsOpen) return;

        for (int i = 0; i < hiddenTargets.Length; i++)
        {
            if (hiddenTargets[i] != null)
            {
                previousStates[i] = hiddenTargets[i].activeSelf;
                hiddenTargets[i].SetActive(false);
            }
        }

        rushObject?.GetComponent<RushTimerUI>()?.SetSuppressed(true);
        magnetObject?.GetComponent<MagnetTimerUI>()?.SetSuppressed(true);

        if (marketPanel != null) marketPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseMarket()
    {
        if (marketPanel != null) marketPanel.SetActive(false);

        for (int i = 0; i < hiddenTargets.Length; i++)
        {
            if (hiddenTargets[i] != null)
                hiddenTargets[i].SetActive(previousStates[i]);
        }

        rushObject?.GetComponent<RushTimerUI>()?.SetSuppressed(false);
        magnetObject?.GetComponent<MagnetTimerUI>()?.SetSuppressed(false);

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
