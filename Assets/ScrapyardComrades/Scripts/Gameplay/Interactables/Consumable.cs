using UnityEngine;

public class Consumable : VoBehavior, Interactable
{
    public SCConsumable Data;

    public void Interact(InteractionController interactor)
    {
        //TODO: Animate this
        interactor.GetComponent<Damagable>().IncreaseMaxHealth(this.Data.MutateAmount, this.Data.HealAmount);
        this.GetComponent<WorldEntity>().TriggerConsumption();
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
