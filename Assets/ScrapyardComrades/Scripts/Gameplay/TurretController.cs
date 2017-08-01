using UnityEngine;
using System.Collections.Generic;

public class TurretController : VoBehavior
{
    public IntegerCollider DetectionRange;
    public LayerMask DetectionMask;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation EAnimation;
    public SCSpriteAnimation NEEAnimation;
    public SCSpriteAnimation NEAnimation;
    public SCSpriteAnimation NNEAnimation;
    public SCSpriteAnimation NAnimation;
    public AttachDir AttachedAt = AttachDir.Down;
    public PooledObject MissilePrefab;
    public int Cooldown = 30;

    [System.Serializable]
    public enum AttachDir
    {
        Down,
        Up,
        Left,
        Right
    }

    void Awake()
    {
        _inRange = new List<GameObject>(1);
        _cooldownTimer = new Timer(this.Cooldown, false, true);
        _cooldownTimer.complete();
    }

    void OnSpawn()
    {
        switch (this.AttachedAt)
        {
            default:
            case AttachDir.Down:
                _normal = Vector2.up;
                break;
            case AttachDir.Up:
                _normal = Vector2.down;
                break;
            case AttachDir.Left:
                _normal = Vector2.right;
                break;
            case AttachDir.Right:
                _normal = Vector2.left;
                break;
        }
    }

    void Update()
    {
        this.DetectionRange.Collide(_inRange, 0, 0, this.DetectionMask);
        if (_inRange.Count > 0)
        {
            float dist = float.MaxValue;
            Transform target = null;
            for (int i = 0; i < _inRange.Count; ++i)
            {
                Transform other = _inRange[i].transform;
                float d = this.transform.Distance2D(other);
                if (d < dist)
                {
                    d = dist;
                    target = other;
                }
            }

            _inRange.Clear();
            aimAtTarget(target);
            attemptFire(target);
        }
    }

    /**
     * Private
     */
    private List<GameObject> _inRange;
    private Timer _cooldownTimer;
    private Vector2 _normal;

    private const float NUM_POS = 5.0f;
    private const float COVERAGE_PER_POS = 22.5f;
    private const float COVERAGE_PER_POS_HALF = 11.25f;
    private const float COVERAGE_TOTAL = 202.5f;
    private const float COVERAGE_HALF = 101.25f;

    private void aimAtTarget(Transform target)
    {
        Vector2 targetDir = this.transform.DirectionTo2D(target);
        float targetAngle = Vector2.Angle(_normal, targetDir);
        float absTargetAngle = Mathf.Abs(targetAngle);
        if (absTargetAngle > COVERAGE_HALF)
        {
            this.Animator.PlayAnimation(this.EAnimation);
        }
        else
        {
            int count = Mathf.RoundToInt(absTargetAngle / NUM_POS);
            switch (count)
            {
                default:
                case 0:
                    this.Animator.PlayAnimation(this.NAnimation);
                    break;
                case 1:
                    this.Animator.PlayAnimation(this.NNEAnimation);
                    break;
                case 2:
                    this.Animator.PlayAnimation(this.NEAnimation);
                    break;
                case 3:
                    this.Animator.PlayAnimation(this.NEEAnimation);
                    break;
                case 4:
                    this.Animator.PlayAnimation(this.EAnimation);
                    break;
            }
        }

        if (absTargetAngle >= COVERAGE_PER_POS_HALF)
            this.spriteRenderer.flipX = Mathf.Sign(targetAngle) > 0 ? true : false;
        this.Animator.Stop();
    }

    private void attemptFire(Transform target)
    {

    }
}
