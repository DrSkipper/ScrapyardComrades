using UnityEngine;

public class BlockHandler : MonoBehaviour
{
    public LocalEventNotifier Notifier;
    public Damagable Damagable;

    public void HandleBlock(int freezeFrames)
    {
        //TODO: Trigger block-stun state & animation

        if (_freezeFrameEvent == null)
            _freezeFrameEvent = new FreezeFrameEvent(freezeFrames);
        else
            _freezeFrameEvent.NumFrames = freezeFrames;

        this.Notifier.SendEvent(_freezeFrameEvent);

        if (this.Damagable != null)
            this.Damagable.SetInvincible(freezeFrames);
    }

    /**
     * Private
     */
    private FreezeFrameEvent _freezeFrameEvent;
}
