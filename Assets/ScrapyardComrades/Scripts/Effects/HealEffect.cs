using UnityEngine;

[RequireComponent(typeof(MaterialLerper))]
public class HealEffect : VoBehavior
{
    public SpriteRenderer SpriteRenderer;
    public MaterialLerper OverlayMaterialLerper;
    public int Duration;

    void Awake()
    {
        this.localNotifier.Listen(HealEvent.NAME, this, onHeal);
        _effectTimer = new Timer(this.Duration, false, false, effectFinished);
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
        _effectTimer.reset();
        _effectTimer.start();
        _prevMat = this.spriteRenderer.material;
        this.spriteRenderer.material = this.OverlayMaterialLerper.Material;
        this.OverlayMaterialLerper.BeginLerp();
    }

    private void effectFinished()
    {
        this.spriteRenderer.material = _prevMat;
    }
}
