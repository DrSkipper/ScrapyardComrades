using UnityEngine;

public class Consumable : VoBehavior, Interactable
{
    public SCConsumable Data;
    public int Bonus = 0;
    public SoundData.Key ConsumptionSfxKey;

    public bool Interact(InteractionController interactor)
    {
        int mutate = this.Data.MutateAmount + this.Bonus;
        int heal = this.Data.HealAmount + this.Bonus;

        WorldEntity entity = this.GetComponent<WorldEntity>();
        if (entity != null && entity.enabled)
            entity.TriggerConsumption();
        else
            ObjectPools.Release(this.gameObject);

        interactor.GetComponent<Damagable>().IncreaseMaxHealth(mutate, heal);
        HeartSpawner heartSpawner = interactor.GetComponent<HeartSpawner>();
        if (heartSpawner != null)
            heartSpawner.Bonus += mutate;

        if (_heartConsumedEvent == null)
            _heartConsumedEvent = new HeartConsumedEvent(this.transform.position);
        else
            _heartConsumedEvent.Position = this.transform.position;

        GlobalEvents.Notifier.SendEvent(_heartConsumedEvent);
        SoundManager.Play(this.ConsumptionSfxKey);
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
