using UnityEngine;

public interface Interactable
{
    void Interact(InteractionController interactor);
}

public class Pickup : MonoBehaviour, Interactable
{
    public SCPickup Data;

    public void Interact(InteractionController interactor)
    {
        interactor.GetComponent<InventoryController>().PickupItem(this.Data);
        ObjectPools.Release(this.gameObject);
    }
}

public class Consumable : MonoBehaviour, Interactable
{
    public SCConsumable Data;

    public void Interact(InteractionController interactor)
    {
        //TODO: Animate this
        // Heal
        interactor.GetComponent<Damagable>().Heal(this.Data.HealAmount);
        //TODO: Mutate
        // Remove this object
        ObjectPools.Release(this.gameObject);
    }
}

public class Talkable : MonoBehaviour, Interactable
{
    public SCDialog Data;

    public void Interact(InteractionController interactor)
    {
        //TODO
    }
}
