using UnityEngine;

public class Pickup : VoBehavior, Interactable, IKey
{
    public SCPickup Data;
    public float MaxVelocityToCatch = 3.0f;

    public bool Interact(InteractionController interactor)
    {
        if (this.Actor.TotalVelocity.magnitude > this.MaxVelocityToCatch)
            return false;

        InventoryController inventory = interactor.GetComponent<InventoryController>();
        if (inventory != null && inventory.NumItems < inventory.InventorySize)
        {
            interactor.GetComponent<InventoryController>().PickupItem(this.Data);
            this.GetComponent<WorldEntity>().TriggerConsumption();
            return true;
        }
        return false;
    }
    
    public void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    public bool CanOpen(SCPickup.KeyType lockType)
    {
        return this.Data.Key != SCPickup.KeyType.None && lockType == this.Data.Key;
    }
}
