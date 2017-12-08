using UnityEngine;

public class TriggerBox : VoBehavior, IPausable
{
    public string EventToSend = "TRIGGER";
    public LayerMask TriggerMask;

    void OnSpawn()
    {
        if (this.spriteRenderer != null)
            this.spriteRenderer.enabled = false;
    }

    void OnReturnToPool()
    {
        if (this.spriteRenderer != null)
            this.spriteRenderer.enabled = true;
    }

    void FixedUpdate()
    {
        if (this.integerCollider.CollideFirst(0, 0, this.TriggerMask))
        {
            LocalEventNotifier.Event e = new LocalEventNotifier.Event();
            e.Name = this.EventToSend;
            GlobalEvents.Notifier.SendEvent(e);
            this.GetComponent<WorldEntity>().TriggerConsumption(true);
        }
    }
}
