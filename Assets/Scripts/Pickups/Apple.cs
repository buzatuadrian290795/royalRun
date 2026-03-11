using UnityEngine;

public class Apple : Pickup
{
    protected override void OnPickup()
    {
        Debug.Log("Add 100 points");
    }
}
