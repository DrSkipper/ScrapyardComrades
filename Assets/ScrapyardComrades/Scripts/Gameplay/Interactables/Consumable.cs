using UnityEngine;

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
