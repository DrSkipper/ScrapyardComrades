using UnityEngine;

public class WorldEntity : VoBehavior
{
    public string QuadName;
    public string EntityName;

    public string StateTag
    {
        get
        {
            return EntityTracker.Instance.GetEntityStateTag(this.QuadName, this.EntityName);
        }
        set
        {
            EntityTracker.Instance.SetEntityStateTag(this.QuadName, this.EntityName, value);
        }
    }

    void Awake()
    {
        _entityConsumedEvent = new EntityConsumedEvent(this.QuadName, this.EntityName);
    }

    public void TriggerConsumption()
    {
        _entityConsumedEvent.QuadName = this.QuadName;
        _entityConsumedEvent.EntityName = this.EntityName;
        GlobalEvents.Notifier.SendEvent(_entityConsumedEvent);
        ObjectPools.Release(this.gameObject);
    }

    /**
     * Private
     */
    private EntityConsumedEvent _entityConsumedEvent;
}
