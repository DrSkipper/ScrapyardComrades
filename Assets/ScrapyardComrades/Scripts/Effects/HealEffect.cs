using UnityEngine;

[RequireComponent(typeof(MaterialLerper))]
public class HealEffect : VoBehavior, IPausable
{
    public SpriteRenderer SpriteRenderer;
    public MaterialLerper OverlayMaterialLerper;
    public int Duration;

    void Awake()
    {
        _prevMat = this.spriteRenderer.material;
        _effectTimer = new Timer(this.Duration, false, false, effectFinished);

        this.localNotifier.Listen(HealEvent.NAME, this, onHeal);
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
    }

    void FixedUpdate()
    {
        _effectTimer.update();
    }

    /**
     * Private
     */
    private Timer _effectTimer;
    private Material _prevMat;

    private void onHeal(LocalEventNotifier.Event e)
    {
        beginEffect();
    }

    private void onHitStun(LocalEventNotifier.Event e)
    {
        if (!(e as HitStunEvent).Blocked)
            beginEffect();
    }

    private void beginEffect()
    {
        _effectTimer.reset();
        _effectTimer.start();
        this.spriteRenderer.material = this.OverlayMaterialLerper.Material;
        this.OverlayMaterialLerper.BeginLerp();
    }

    private void effectFinished()
    {
        this.spriteRenderer.material = _prevMat;
    }
}
