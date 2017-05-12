using UnityEngine;

public class Consumable : VoBehavior, Interactable
{
    public SCConsumable Data;

    public void Interact(InteractionController interactor)
    {
        interactor.GetComponent<Damagable>().IncreaseMaxHealth(this.Data.MutateAmount, this.Data.HealAmount);
        WorldEntity entity = this.GetComponent<WorldEntity>();
        if (entity != null && entity.enabled)
            entity.TriggerConsumption();
        else
            ObjectPools.Release(this.gameObject);
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
