using UnityEngine;

public class DestroyOnCollision : VoBehavior, IPausable
{
    public LayerMask DestructionLayers;
    public int DelayBeforeChecking = 10;
    public int DelayBeforeDestruction = 10;

    void Awake()
    {
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    void OnSpawn()
    {
        _a = 0.0f;
        updateAlpha();

        if (_fadeInTimer == null)
            _fadeInTimer = new Timer(this.DelayBeforeChecking, false, true);
        else
            _fadeInTimer.resetAndStart(this.DelayBeforeChecking);

        if (_fadeOutTimer == null)
            _fadeOutTimer = new Timer(this.DelayBeforeDestruction, false, false, onFadeOutComplete);
        else
        {
            _fadeOutTimer.Paused = true;
            _fadeOutTimer.reset(this.DelayBeforeDestruction);
        }

    }

    void FixedUpdate()
    {
        _fadeInTimer.update();
        _fadeOutTimer.update();

        if (!_fadeInTimer.Completed)
            _a = 1.0f - ((float)_fadeInTimer.FramesRemaining / this.DelayBeforeChecking);
        else if (_fadeOutTimer.IsRunning)
            _a = ((float)_fadeOutTimer.FramesRemaining / this.DelayBeforeChecking);
        else if (_fadeOutTimer.Completed)
            _a = 0.0f;
        else
            _a = 1.0f;

        updateAlpha();
    }

    /**
     * Private
     */
    private Timer _fadeInTimer;
    private Timer _fadeOutTimer;
    float _a;

    private void onCollide(LocalEventNotifier.Event e)
    {
        if (!_fadeInTimer.Completed)
            return;

        CollisionEvent ce = e as CollisionEvent;

        for (int i = 0; i < ce.Hits.Count; ++i)
        {
            if (this.DestructionLayers.ContainsLayer(ce.Hits[i].layer))
            {
                _fadeOutTimer.start();
                break;
            }
        }
    }

    private void updateAlpha()
    {
        Color c = this.spriteRenderer.color;
        c.a = _a;
        this.spriteRenderer.color = c;
    }

    private void onFadeOutComplete()
    {
        ObjectPools.Release(this.gameObject);
    }
}
