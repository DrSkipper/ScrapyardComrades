using UnityEngine;

public class FallenParamedic : VoBehavior, IPausable
{
    public Sprite PreInteractSprite;
    public Sprite PostInteractSprite;

    void OnSpawn()
    {
        _added = false;
        if (SaveData.DataLoaded)
        {
            if (SaveData.PlayerStats.Level > 0)
            {
                this.spriteRenderer.sprite = this.PostInteractSprite;
            }
            else
            {
                this.spriteRenderer.sprite = this.PreInteractSprite;
                _added = true;
                GlobalEvents.Notifier.Listen(PlayerHealthController.MUTATE_EVENT, this, onMutate);
            }
        }
    }

    void OnReturnToPool()
    {
        if (_added)
            GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, PlayerHealthController.MUTATE_EVENT);
    }

    /**
     * Private
     */
    private bool _added;

    private void onMutate(LocalEventNotifier.Event e)
    {
        this.spriteRenderer.sprite = this.PreInteractSprite;
    }
}
