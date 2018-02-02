using UnityEngine;

public class TriggerBox : VoBehavior, IPausable
{
    public string StateKeyToSetOn = "TRIGGER";
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
            GlobalEvents.Notifier.SendEvent(new SwitchStateChangedEvent(this.StateKeyToSetOn, Switch.SwitchState.ON), true);
            SaveData.SetGlobalState(this.StateKeyToSetOn, Switch.ON);
            this.GetComponent<WorldEntity>().TriggerConsumption(true);
        }
    }
}
