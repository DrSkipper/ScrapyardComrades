using UnityEngine;

[RequireComponent(typeof(MaterialLerper))]
public class HealEffect : VoBehavior, IPausable
{
    public SpriteRenderer SpriteRenderer;
    public MaterialLerper OverlayMaterialLerper;
    public Material BlockMaterial;
    public float HealthStartAlpha = 0.9f;
    public float BlockStartAlpha = 0.7f;
    public int Duration;

    void Awake()
    {
        _healthMaterial = this.OverlayMaterialLerper.Material;
        _prevMat = this.spriteRenderer.material;
        _effectTimer = new Timer(this.Duration, false, false, effectFinished);

        this.localNotifier.Listen(HealEvent.NAME, this, onHeal);
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
    }

    void FixedUpdate()
    {
        _effectTimer.update();
    }
    
    public void BeginEffect()
    {
        _effectTimer.reset();
        _effectTimer.start();
        this.spriteRenderer.material = this.OverlayMaterialLerper.Material;
        this.OverlayMaterialLerper.BeginLerp();
    }

    /**
     * Private
     */
    private Timer _effectTimer;
    private Material _prevMat;
    private Material _healthMaterial;

    private void onHeal(LocalEventNotifier.Event e)
    {
        this.BeginEffect();
    }

    private void onHitStun(LocalEventNotifier.Event e)
    {
        if ((e as HitStunEvent).Blocked && this.BlockMaterial != null)
        {
            this.OverlayMaterialLerper.Material = this.BlockMaterial;
            this.OverlayMaterialLerper.InitialValue = this.BlockStartAlpha;
        }
        else
        {
            this.OverlayMaterialLerper.Material = _healthMaterial;
            this.OverlayMaterialLerper.InitialValue = this.HealthStartAlpha;
        }

        this.BeginEffect();
    }

    private void effectFinished()
    {
        this.spriteRenderer.material = _prevMat;
    }
}
