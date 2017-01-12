using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    public SCPickup Data;

    public void Interact(InteractionController interactor)
    {
        interactor.GetComponent<InventoryController>().PickupItem(this.Data);
        ObjectPools.Release(this.gameObject);
    }
}
