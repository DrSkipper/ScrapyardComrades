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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3)_normal * 80);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3)_lastAimDir * 80);
    }

    /**
     * Private
     */
    private List<GameObject> _inRange;
    private Timer _cooldownTimer;
    private Vector2 _normal;
    private Vector2 _lastAimDir;
    
    private const float COVERAGE_PER_POS = 22.5f;
    private const float COVERAGE_PER_POS_HALF = 11.25f;
    private const float COVERAGE_TOTAL = 202.5f;
    private const float COVERAGE_HALF = 101.25f;
    private const float MIN_AMT_FOR_TURN = 5.0f;

    private void aimAtTarget(Transform target)
    {
        Vector2 targetDir = this.transform.DirectionTo2D(target);
        _lastAimDir = targetDir;
        float absTargetAngle = Vector2.Angle(_normal, targetDir);
        float targetAngle = absTargetAngle * getSign(target.transform.position);

        if (absTargetAngle > COVERAGE_HALF)
        {
            this.Animator.PlayAnimation(this.EAnimation);
        }
        else
        {
            int count = Mathf.RoundToInt(absTargetAngle / COVERAGE_PER_POS);
            switch (count)
            {
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
                default:
                case 4:
                    this.Animator.PlayAnimation(this.EAnimation);
                    break;
            }
        }

        if (absTargetAngle >= MIN_AMT_FOR_TURN)
            this.spriteRenderer.flipX = Mathf.Sign(targetAngle) > 0 ? true : false;
        this.Animator.Stop();
    }

    private void attemptFire(Transform target)
    {

    }

    private int getSign(Vector2 target)
    {
        switch (this.AttachedAt)
        {
            default:
            case AttachDir.Down:
                return target.x < this.transform.position.x ? -1 : 1;
            case AttachDir.Up:
                return target.x > this.transform.position.x ? -1 : 1;
            case AttachDir.Left:
                return target.y < this.transform.position.x ? -1 : 1;
            case AttachDir.Right:
                return target.y > this.transform.position.x ? -1 : 1;
        }
    }
}
