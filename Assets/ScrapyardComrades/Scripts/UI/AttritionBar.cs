using UnityEngine;

public class AttritionBar : MonoBehaviour
{
    public UIBar TimeLeftBar;
    public SCSpriteAnimator TimeIconAnimator;
    public SCSpriteAnimation TimeIconIdleAnim;
    public SCSpriteAnimation TimeIconResetAnim;
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
            if (!this.TimeIconAnimator.IsPlaying)
                this.TimeIconAnimator.PlayAnimation(this.TimeIconIdleAnim);
        }
    }

    /**
     * Private
     */
    private PlayerHealthController _healthController;

    private void onPlayerSpawned(LocalEventNotifier.Event e)
    {
        _healthController = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<PlayerHealthController>();
        _healthController.HealthChangedCallback += onHealthChanged;
        this.TimeLeftBar.ChangeTargetLength(Mathf.RoundToInt(this.PixelUnitsPerAttritionUnit * _healthController.AttritionInterval));
        this.TimeLeftBar.UpdateLength(_healthController.AttritionTimeLeft, _healthController.AttritionInterval);
    }

    private void onHealthChanged(int currentHealth, int maxHealth, bool wasAttrition, bool animate = true, bool highlight = true)
    {
        if (wasAttrition)
        {
            this.TimeIconAnimator.PlayAnimation(this.TimeIconResetAnim);
        }
    }
}
