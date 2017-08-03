using UnityEngine;
using System.Collections.Generic;

public class TurretController : VoBehavior, IPausable
{
    public IntegerCollider DetectionRange;
    public LayerMask DetectionMask;
    public LayerMask BlockMask;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation EAnimation;
    public SCSpriteAnimation NEEAnimation;
    public SCSpriteAnimation NEAnimation;
    public SCSpriteAnimation NNEAnimation;
    public SCSpriteAnimation NAnimation;
    public SCSpriteAnimation TurnAnimation;
    public AttachDir AttachedAt = AttachDir.Down;
    public PooledObject MissilePrefab;
    public SCSpriteAnimator EffectAnimator;
    public Transform LaunchOrigin;
    public int Cooldown = 30;
    public int ShotDelay = 3;
    public int ShotStartDistance = 24;
    public float MissileRotationOffset = -90.0f;

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
        _shotDelayTimer = new Timer(this.ShotDelay, false, false, shoot);
    }

    void OnSpawn()
    {
        _cooldownTimer.complete();
        this.EffectAnimator.gameObject.SetActive(false);

        switch (this.AttachedAt)
        {
            default:
            case AttachDir.Down:
                _normal = Vector2.up;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case AttachDir.Up:
                _normal = Vector2.down;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case AttachDir.Left:
                _normal = Vector2.right;
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case AttachDir.Right:
                _normal = Vector2.left;
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
    }

    void FixedUpdate()
    {
        _cooldownTimer.update();
        _shotDelayTimer.update();

        if (!_isFiring && !_shotDelayTimer.IsRunning)
        {
            this.DetectionRange.Collide(_inRange, 0, 0, this.DetectionMask);
            bool found = false;
            if (_inRange.Count > 0)
            {
                float dist = float.MaxValue;
                Transform target = null;
                Vector2 targetDir = Vector2.zero;

                for (int i = 0; i < _inRange.Count; ++i)
                {
                    Transform other = _inRange[i].transform;
                    float d = this.LaunchOrigin.Distance2D(other);
                    if (d < dist)
                    {
                        Vector2 dir = this.LaunchOrigin.DirectionTo2D(other);
                        CollisionManager.RaycastResult result = CollisionManager.RaycastFirst((Vector2)this.LaunchOrigin.transform.position, dir, this.LaunchOrigin.Distance2D(other), this.BlockMask);

                        if (!result.Collided || result.Collisions[0].CollidedObject == other.gameObject)
                        {
                            found = true;
                            d = dist;
                            target = other;
                            targetDir = dir;
                        }
                    }
                }

                _inRange.Clear();

                if (found)
                {
                    aimAtTarget(target, targetDir);
                    attemptFire(target, targetDir);
                }
            }

            if (!found)
            {
                _cooldownTimer.reset();
                _cooldownTimer.start();
            }
        }
        else if (!this.Animator.IsPlaying || _cooldownTimer.Completed)
        {
            _isFiring = false;
        }

        if (_turning)
        {
            if (!this.Animator.IsPlaying)
            {
                _turning = false;
                spriteRenderer.flipX = _turnTo;
            }
            else if (this.Animator.Elapsed >= this.TurnAnimation.LengthInFrames / 2)
            {
                spriteRenderer.flipX = _turnTo;
            }
        }

        if (!this.EffectAnimator.IsPlaying && this.EffectAnimator.gameObject.activeInHierarchy)
            this.EffectAnimator.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3)_normal * 80);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3)_lastAimDir * 80);
    }

    void OnReturnToPool()
    {
        _cooldownTimer.reset();
        _shotDelayTimer.reset();
        _shotDelayTimer.Paused = true;
    }

    /**
     * Private
     */
    private List<GameObject> _inRange;
    private Timer _cooldownTimer;
    private Timer _shotDelayTimer;
    private Vector2 _normal;
    private Vector2 _lastAimDir;
    private bool _isFiring;
    private bool _firableDirection;
    private Vector2 _shotTarget;
    private Vector2 _shotDir;
    private bool _turning;
    private bool _turnTo;
    private int _currentPos;
    
    private const float COVERAGE_PER_POS = 22.5f;
    private const float COVERAGE_PER_POS_HALF = 11.25f;
    private const float COVERAGE_TOTAL = 202.5f;
    private const float COVERAGE_HALF = 101.25f;
    private const float MIN_AMT_FOR_TURN = 5.0f;
    private const float EFFECT_X_MULT = 1.6f;

    private void aimAtTarget(Transform target, Vector2 targetDir)
    {
        _lastAimDir = targetDir;
        float absTargetAngle = Vector2.Angle(_normal, targetDir);
        float targetAngle = absTargetAngle * getSign(target.transform.position);
        if (absTargetAngle > COVERAGE_HALF)
        {
            _firableDirection = false;
            _currentPos = 5;
        }
        else
        {
            _firableDirection = true;
            _currentPos = Mathf.RoundToInt(absTargetAngle / COVERAGE_PER_POS);
        }
        
        if (!_turning && _firableDirection && absTargetAngle >= MIN_AMT_FOR_TURN)
        {
            bool flip = Mathf.Sign(targetAngle) > 0 ? true : false;
            if (flip != this.spriteRenderer.flipX)
            {
                this.Animator.PlayAnimation(this.TurnAnimation);
                _turning = true;
                _turnTo = flip;
            }
        }

        if (!_turning)
            playAnimForCurrentPos(false);
    }

    private void attemptFire(Transform target, Vector2 targetDir)
    {
        if (_cooldownTimer.Completed && _firableDirection)
        {
            if (_turning)
            {
                _turning = false;
                this.spriteRenderer.flipX = _turnTo;
            }

            _isFiring = true;
            _cooldownTimer.reset();
            _cooldownTimer.start();
            playAnimForCurrentPos(true);

            _shotTarget = target.transform.position;
            _shotDir = targetDir;
            _shotDelayTimer.reset();
            _shotDelayTimer.start();
        }
    }

    private void shoot()
    {
        IntegerVector pos = (IntegerVector)(Vector2)this.LaunchOrigin.position + (IntegerVector)(_shotDir * this.ShotStartDistance);
        PooledObject missile = this.MissilePrefab.Retain();
        missile.transform.SetPosition2D(pos);
        missile.transform.LookAt2D(_shotTarget, this.MissileRotationOffset);
        missile.GetComponent<ThrownActor>().Throw(_shotDir);
        missile.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        this.EffectAnimator.gameObject.SetActive(true);
        this.EffectAnimator.transform.SetPosition2D(pos);
        this.EffectAnimator.transform.SetLocalX(Mathf.Round(this.EffectAnimator.transform.localPosition.x * EFFECT_X_MULT));
        this.EffectAnimator.spriteRenderer.flipX = this.spriteRenderer.flipX;
        this.EffectAnimator.Play();
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
                return target.y > this.transform.position.y ? -1 : 1;
            case AttachDir.Right:
                return target.y < this.transform.position.y ? -1 : 1;
        }
    }

    private void playAnimForCurrentPos(bool fire)
    {
        switch (_currentPos)
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

        if (!fire)
            this.Animator.Stop();
    }
}
