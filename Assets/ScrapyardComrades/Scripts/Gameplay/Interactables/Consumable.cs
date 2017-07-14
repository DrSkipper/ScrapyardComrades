using UnityEngine;

public class Consumable : VoBehavior, Interactable
{
    public SCConsumable Data;
    public AudioClip ConsumptionSound;

    public bool Interact(InteractionController interactor)
    {
        int mutate = this.Data.MutateAmount;
        int heal = this.Data.HealAmount;

        WorldEntity entity = this.GetComponent<WorldEntity>();
        if (entity != null && entity.enabled)
            entity.TriggerConsumption();
        else
            ObjectPools.Release(this.gameObject);

        interactor.GetComponent<Damagable>().IncreaseMaxHealth(mutate, heal);

        if (_heartConsumedEvent == null)
            _heartConsumedEvent = new HeartConsumedEvent(this.transform.position);
        else
            _heartConsumedEvent.Position = this.transform.position;

        GlobalEvents.Notifier.SendEvent(_heartConsumedEvent);

        if (this.ConsumptionSound != null)
            SoundManager.Play(this.ConsumptionSound.name);
        return true;
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
