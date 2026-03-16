using System.Globalization;
using TMPro;
using UnityEngine;

public class DistanceMeter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private PlayerRespawnManager playerRespawnManager;

    private float distanceTravelled;

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
        if (distanceText == null || levelGenerator == null)
        {
            return;
        }

        if (IsPlayerDead())
        {
            return;
        }

        distanceTravelled += levelGenerator.MoveSpeed / 4 * Time.fixedDeltaTime;

        long meters = Mathf.FloorToInt(distanceTravelled);
        distanceText.text = FormatDistance(meters);
    }

    private bool IsPlayerDead()
    {
        if (playerRespawnManager == null)
        {
            return false;
        }

        GameObject currentPlayer = playerRespawnManager.CurrentPlayer;
        if (currentPlayer == null)
        {
            return true;
        }

        RagdollController ragdollController = currentPlayer.GetComponent<RagdollController>();
        if (ragdollController == null)
        {
            return false;
        }

        return ragdollController.IsRagdollActive;
    }

    public void ResetDistance()
    {
        distanceTravelled = 0f;

        if (distanceText != null)
        {
            distanceText.text = FormatDistance(0);
        }
    }

    private string FormatDistance(long meters)
    {
        if (meters >= 1000000)
        {
            float millions = meters / 1000000f;
            return millions.ToString("F2") + " M";
        }

        return meters.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ") + " m";
    }
}