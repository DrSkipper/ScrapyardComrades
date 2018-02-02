using UnityEngine;

public class WorldEntity : VoBehavior
{
    public string QuadName;
    public string EntityName;

    public string StateTag
    {
        get
        {
            if (EntityTracker.Instance == null)
                return StringExtensions.EMPTY;
            return EntityTracker.Instance.GetEntityStateTag(this.QuadName, this.EntityName);
        }
        set
        {
            if (EntityTracker.Instance != null)
                EntityTracker.Instance.SetEntityStateTag(this.QuadName, this.EntityName, value);
        }
    }

    void Awake()
    {
        _entityConsumedEvent = new EntityConsumedEvent(this.QuadName, this.EntityName);
    }

    public void TriggerConsumption(bool releaseObject = true)
    {
        _entityConsumedEvent.QuadName = this.QuadName;
        _entityConsumedEvent.EntityName = this.EntityName;
        GlobalEvents.Notifier.SendEvent(_entityConsumedEvent);

        if (releaseObject)
            ObjectPools.Release(this.gameObject);
    }

    /**
     * Private
     */
    private EntityConsumedEvent _entityConsumedEvent;
}
