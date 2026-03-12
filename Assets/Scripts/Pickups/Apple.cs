using UnityEngine;

public class Apple : Pickup
{
    protected override void OnPickup()
    {
        Debug.Log("Add Vitamins");
    }
}
