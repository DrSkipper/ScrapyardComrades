using UnityEngine;

public class OLDParallaxLayer : VoBehavior, IPausable
{
    public Vector2 ParallaxMultiplier = new Vector2(0.5f, 0.5f);

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    //TODO: fixed update?
    void Update()
    {
        if (_tracker != null)
        {
            Vector2 trackerPosition = (Vector2)_tracker.transform.position;
            Vector2 diff = trackerPosition - _previousPosition;
            this.transform.localPosition -= new Vector3(diff.x * this.ParallaxMultiplier.x, diff.y * this.ParallaxMultiplier.y, 0);
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
