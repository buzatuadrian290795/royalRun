using System.Globalization;
using TMPro;
using UnityEngine;

public class DistanceMeter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private RagdollController ragdollController;

    private float distanceTravelled;

    private void FixedUpdate()
    {
        if (distanceText == null) return;
        if (ragdollController != null && ragdollController.IsRagdollActive) return;

        distanceTravelled += moveSpeed * Time.deltaTime;

        long meters = Mathf.FloorToInt(distanceTravelled);
        distanceText.text = FormatDistance(meters);
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