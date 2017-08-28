using UnityEngine;

public class AttritionBar : MonoBehaviour
{
    public UIBar TimeLeftBar;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, onPlayerSpawned);
    }

    void FixedUpdate()
    {
        if (_healthController != null)
        {
            this.TimeLeftBar.UpdateLength(_healthController.AttritionTimeLeft, _healthController.AttritionInterval);
        }
    }

    /**
     * Private
     */
    private PlayerHealthController _healthController;

    private void onPlayerSpawned(LocalEventNotifier.Event e)
    {
        _healthController = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<PlayerHealthController>();
    }
}
