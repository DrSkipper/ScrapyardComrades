using UnityEngine;

public class Pickup : VoBehavior, Interactable, ISpawnable
{
    public SCPickup Data;

    public void Interact(InteractionController interactor)
    {
        InventoryController inventory = interactor.GetComponent<InventoryController>();
        if (inventory.NumItems < inventory.InventorySize)
        {
            interactor.GetComponent<InventoryController>().PickupItem(this.Data);
            this.GetComponent<WorldEntity>().TriggerConsumption();
        }
    }
    
    public void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }
}
