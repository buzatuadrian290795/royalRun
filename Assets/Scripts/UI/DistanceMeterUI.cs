using System.Globalization;
using TMPro;
using UnityEngine;

public class DistanceMeterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private PlayerRespawnManager playerRespawnManager;
    [SerializeField] private float speedToDistanceRatio = 4f;

    private long lastMeters = -1;
    private float distanceTravelled;

    private RagdollController cachedRagdoll;
    private GameObject cachedPlayer;

    private void Awake()
    {
        if (levelGenerator == null)
        {
            levelGenerator = FindFirstObjectByType<LevelGenerator>();
        }

        if (playerRespawnManager == null)
        {
            playerRespawnManager = FindFirstObjectByType<PlayerRespawnManager>();
        }

        if (distanceText == null)
        {
            Debug.LogError("DistanceMeter: distanceText is not set.");
        }

        if (levelGenerator == null)
        {
            Debug.LogError("DistanceMeter: LevelGenerator not found.");
        }

        if (playerRespawnManager == null)
        {
            Debug.LogError("DistanceMeter: PlayerRespawnManager not found.");
        }
    }

    private void FixedUpdate()
    {
        if (distanceText == null || levelGenerator == null || playerRespawnManager == null)
            return;

        if (IsPlayerDead())
        {
            return;
        }

        distanceTravelled += levelGenerator.MoveSpeed / speedToDistanceRatio * Time.fixedDeltaTime;

        long meters = Mathf.FloorToInt(distanceTravelled);
        if (meters != lastMeters)
        {
            lastMeters = meters;
            distanceText.text = FormatDistance(meters);
        }
    }

    private bool IsPlayerDead()
    {
        GameObject currentPlayer = playerRespawnManager?.CurrentPlayer;
        if (currentPlayer == null) return true;

        if (currentPlayer != cachedPlayer)
        {
            cachedPlayer = currentPlayer;
            cachedRagdoll = currentPlayer.GetComponent<RagdollController>();
        }

        return cachedRagdoll != null && cachedRagdoll.IsRagdollActive;
    }

    //public void ResetDistance()
    //{
    //    distanceTravelled = 0f;
    //    cachedPlayer = null;
    //    cachedRagdoll = null;
    //    if (distanceText != null) 
    //    {
    //    distanceText.text = FormatDistance(0); 
    //    }
    //    lastMeters = 0;
    //}

    private string FormatDistance(long meters)
    {
        if (meters >= 1_000_000)
        {
            return (meters / 1_000_000f).ToString("F2") + " Mm";
        }
        if (meters >= 1_000)
        {
            return (meters / 1_000f).ToString("F2") + " km";
        }
        return meters.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ") + " m";
    }
}