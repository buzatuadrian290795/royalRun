using System.Globalization;
using TMPro;
using UnityEngine;

// Masoara si afiseaza distanta parcursa de jucator in timp real
public class DistanceMeterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private PlayerRespawnManager playerRespawnManager;

    // Cat de multi metri reprezinta o unitate de viteza (regleaza din Inspector)
    [SerializeField] private float speedToDistanceRatio = 4f;

    private long lastMeters = -1;       // Ultimul numar de metri afisati (evita update inutil)
    private float distanceTravelled;     // Distanta acumulata in unitati float

    // Cache pentru jucatorul curent si ragdoll-ul sau
    private RagdollController cachedRagdoll;
    private GameObject cachedPlayer;

    private void Awake()
    {
        if (levelGenerator == null)
            levelGenerator = FindFirstObjectByType<LevelGenerator>();
        if (playerRespawnManager == null)
            playerRespawnManager = FindFirstObjectByType<PlayerRespawnManager>();

        if (distanceText == null) Debug.LogError("DistanceMeter: distanceText is not set.");
        if (levelGenerator == null) Debug.LogError("DistanceMeter: LevelGenerator not found.");
        if (playerRespawnManager == null) Debug.LogError("DistanceMeter: PlayerRespawnManager not found.");
    }

    private void FixedUpdate()
    {
        if (distanceText == null || levelGenerator == null || playerRespawnManager == null) return;
        if (IsPlayerDead()) return;

        // Adauga distanta proportional cu viteza curenta a chunk-urilor
        distanceTravelled += levelGenerator.MoveSpeed / speedToDistanceRatio * Time.fixedDeltaTime;

        long meters = Mathf.FloorToInt(distanceTravelled);

        // Actualizeaza textul doar cand se schimba numarul de metri (nu la fiecare frame)
        if (meters != lastMeters)
        {
            lastMeters = meters;
            distanceText.text = FormatDistance(meters);
        }
    }

    // Returneaza true daca jucatorul e mort (ragdoll activ) sau inexistent
    private bool IsPlayerDead()
    {
        GameObject currentPlayer = playerRespawnManager?.CurrentPlayer;
        if (currentPlayer == null) return true;

        // Re-cache-uieste componenta daca jucatorul s-a schimbat (ex: dupa respawn)
        if (currentPlayer != cachedPlayer)
        {
            cachedPlayer = currentPlayer;
            cachedRagdoll = currentPlayer.GetComponent<RagdollController>();
        }

        return cachedRagdoll != null && cachedRagdoll.IsRagdollActive;
    }

    // Reseteaza contorul (apelat din SettingsManager la reset progres)
    public void ResetDistance()
    {
        distanceTravelled = 0f;
        cachedPlayer = null;
        cachedRagdoll = null;
        lastMeters = 0;
        if (distanceText != null)
            distanceText.text = FormatDistance(0);
    }

    // Formateaza distanta cu unitati adaptive: m / km / Mm
    private string FormatDistance(long meters)
    {
        if (meters >= 1_000_000)
            return (meters / 1_000_000f).ToString("F2") + " Mm";
        if (meters >= 1_000)
            return (meters / 1_000f).ToString("F2") + " km";

        // Separator de mii cu spatiu (ex: "1 234 m")
        return meters.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ") + " m";
    }
}