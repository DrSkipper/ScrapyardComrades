using UnityEngine;

public class ParallaxLayer : VoBehavior
{
    public float ParallaxMultiplier = 0.5f;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    void Update()
    {
        if (_tracker != null)
        {
            Vector2 trackerPosition = (Vector2)_tracker.transform.position;
            this.transform.localPosition -= (Vector3)((trackerPosition - _previousPosition) * this.ParallaxMultiplier);
            _previousPosition = trackerPosition;
        }
    }

    /**
     * Private
     */
    private Transform _tracker;
    private Vector2 _previousPosition;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _tracker = (e as PlayerSpawnedEvent).PlayerObject.transform;
        _previousPosition = _tracker.transform.position;
    }

    private void onRecenter(LocalEventNotifier.Event e)
    {
        _previousPosition += (Vector2)((e as WorldRecenterEvent).RecenterOffset);
    }
}
