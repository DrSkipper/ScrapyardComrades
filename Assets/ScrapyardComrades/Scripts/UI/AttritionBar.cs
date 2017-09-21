using UnityEngine;

public class AttritionBar : MonoBehaviour
{
    public UIBar TimeLeftBar;
    public float PixelUnitsPerAttritionUnit = 0.5f;

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
        this.TimeLeftBar.ChangeTargetLength(Mathf.RoundToInt(this.PixelUnitsPerAttritionUnit * _healthController.AttritionInterval));
        this.TimeLeftBar.UpdateLength(_healthController.AttritionTimeLeft, _healthController.AttritionInterval);
    }
}
