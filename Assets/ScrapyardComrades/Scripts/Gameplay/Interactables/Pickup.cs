using UnityEngine;

public class Pickup : VoBehavior, Interactable, IKey
{
    public SCPickup Data;
    public LayerMask KeyStopperMask;
    public float MaxVelocityToCatch = 3.0f;

    void Awake()
    {
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

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

    /**
     * Private
     */
    private void onCollide(LocalEventNotifier.Event e)
    {
        if (this.Data.Key == SCPickup.KeyType.None)
            return;

        CollisionEvent collisionEvent = e as CollisionEvent;
        for (int i = 0; i < collisionEvent.Hits.Count; ++i)
        {
            GameObject hit = collisionEvent.Hits[i];
            if (this.KeyStopperMask.ContainsLayer(hit.layer))
            {
                KeyStopper stopper = hit.GetComponent<KeyStopper>();
                if (stopper != null && this.CanOpen(stopper.Door.LockType))
                {
                    this.Actor.Velocity = Vector2.zero;
                    stopper.HandleStop();
                }
            }
        }
    }
}
