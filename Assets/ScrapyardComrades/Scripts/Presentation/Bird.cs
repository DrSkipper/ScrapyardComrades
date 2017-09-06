using UnityEngine;

public class Bird : MonoBehaviour, IPausable
{
    public LayerMask ChaserLayers;
    public IntegerCollider ChaseAwayBounds;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation LookAnimation;
    public SCSpriteAnimation PeckAnimation;
    public SCSpriteAnimation FlyAnimation;
    public IntegerVector IdleTimeRange;
    public IntegerVector LookTimeRange;
    public IntegerVector PeckTimeRange;
    public IntegerVector FlyAwayVelocity;
    public int FlyAwayDuration = 200;

    void Awake()
    {
        _timer = new Timer(this.LookTimeRange.X, false, true);
        _timer.complete();
    }

    void OnSpawn()
    {
        _flying = false;
        _timer.complete();
    }

    void FixedUpdate()
    {
        _timer.update();

        if (!_flying)
        {
            if (this.ChaseAwayBounds.CollideFirst(0, 0, this.ChaserLayers) != null)
            {
                _flying = true;
                this.Animator.PlayAnimation(this.FlyAnimation);
                _delta = this.FlyAwayVelocity;
                _delta.X = Mathf.RoundToInt(this.transform.localScale.x);

                _timer.reset(this.FlyAwayDuration);
                _timer.start();
            }
            else if (_timer.Completed)
            {
                int anim = Random.Range(0, NUM_ANIM_CHOICES);

                switch (anim)
                {
                    default:
                    case IDLE:
                        this.Animator.PlayAnimation(this.IdleAnimation);
                        _timer.reset(Random.Range(this.IdleTimeRange.X, this.IdleTimeRange.Y));
                        break;
                    case LOOK:
                        this.Animator.PlayAnimation(this.LookAnimation);
                        _timer.reset(Random.Range(this.LookTimeRange.X, this.LookTimeRange.Y));
                        break;
                    case PECK:
                        this.Animator.PlayAnimation(this.PeckAnimation);
                        _timer.reset(Random.Range(this.PeckTimeRange.X, this.PeckTimeRange.Y));
                        break;
                }

                _timer.start();
            }
        }
        else
        {
            if (!_timer.Completed)
                this.transform.SetPosition2D((IntegerVector)((Vector2)this.transform.position + (Vector2)this.FlyAwayVelocity));
        }
    }

    /**
     * Private
     */
    private bool _flying;
    private Timer _timer;
    private IntegerVector _delta;

    private const int IDLE = 0;
    private const int LOOK = 1;
    private const int PECK = 2;
    private const int NUM_ANIM_CHOICES = PECK + 1;
}
