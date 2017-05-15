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

        if (_heartConsumedEvent == null)
            _heartConsumedEvent = new HeartConsumedEvent(this.transform.position);
        else
            _heartConsumedEvent.Position = this.transform.position;

        GlobalEvents.Notifier.SendEvent(_heartConsumedEvent);
    }

    public void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    /**
     * Private
     */
    private HeartConsumedEvent _heartConsumedEvent;
}
