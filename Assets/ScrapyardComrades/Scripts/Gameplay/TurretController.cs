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
    public IntegerCollider Hurtbox;
    public PooledObject DeathAnimObject;
    public string InitialState = DIRECT_AIM;
    public IntegerCollider[] SearchColliders;
    public IntegerCollider[] ExitRangeColliders;
    public float SearchSpeed = 10.0f;
    public float MaxAngle = 85.0f;
    public Transform LightTransform;
    public Light SearchLight;
    public Light TargetLight;
    public bool AttachToSurfaces = true;
    public LayerMask SurfaceLayers;
    public SoundData.Key LockOnSfxKey;
    public SoundData.Key UnlockSfxKey;
    public SoundData.Key MoveSfxKey;

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
        _hurtboxOffset = this.Hurtbox.Offset;
        this.GetComponent<Damagable>().OnDeathCallback += onDeath;
        _stateMachine = new FSMStateMachine();
        _stateMachine.AddState(DIRECT_AIM, updateDirectAim);
        _stateMachine.AddState(SEARCHING, updateSearching, enterSearching);
        _stateMachine.AddState(TARGETTING, updateTargetting, enterTargetting, exitTargetting);
        this.localNotifier.Listen(FreezeFrameEvent.NAME, this, freezeFrame);

        _searchColliderDists = new float[this.SearchColliders.Length];
        _exitColliderDists = new float[this.ExitRangeColliders.Length];

        for (int i = 0; i < this.SearchColliders.Length; ++i)
        {
            _searchColliderDists[i] = this.LaunchOrigin.Distance2D(this.SearchColliders[i].transform);
        }

        for (int i = 0; i < this.ExitRangeColliders.Length; ++i)
        {
            _exitColliderDists[i] = this.LaunchOrigin.Distance2D(this.ExitRangeColliders[i].transform);
        }
    }

    void OnSpawn()
    {
        _hasAttached = !this.AttachToSurfaces;
        this.Hurtbox.AddToCollisionPool();
        _cooldownTimer.complete();
        this.EffectAnimator.gameObject.SetActive(false);
        _lastAngle = 0;
        _searchDir = Random.Range(0, 2) == 0 ? -1 : 1;
        enableLight(true);

        switch (this.AttachedAt)
        {
            default:
            case AttachDir.Down:
                _normal = Vector2.up;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                this.Hurtbox.Offset = _hurtboxOffset;
                break;
            case AttachDir.Up:
                _normal = Vector2.down;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                this.Hurtbox.Offset = new IntegerVector(-_hurtboxOffset.X, -_hurtboxOffset.Y);
                break;
            case AttachDir.Left:
                _normal = Vector2.right;
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                this.Hurtbox.Offset = new IntegerVector(_hurtboxOffset.Y, -_hurtboxOffset.X);
                break;
            case AttachDir.Right:
                _normal = Vector2.left;
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                this.Hurtbox.Offset = new IntegerVector(-_hurtboxOffset.Y, _hurtboxOffset.X);
                break;
        }

        _stateMachine.BeginWithInitialState(this.InitialState);

        if (_freezeFrameTimer == null)
            _freezeFrameTimer = new Timer(1);
        _freezeFrameTimer.complete();
    }

    void FixedUpdate()
    {
        if (!_hasAttached)
            attachToSurface();

        if (!_freezeFrameTimer.Completed)
        {
            _freezeFrameTimer.update();
            if (!_freezeFrameTimer.Completed)
                return;
            else
                this.localNotifier.SendEvent(new FreezeFrameEndedEvent());
        }

        _cooldownTimer.update();
        _shotDelayTimer.update();
        _stateMachine.Update();

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
        this.Hurtbox.RemoveFromCollisionPool();
        _cooldownTimer.reset(this.Cooldown);
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
    private IntegerVector _hurtboxOffset;
    private FSMStateMachine _stateMachine;
    private float _lastAngle;
    private int _searchDir;
    private Timer _freezeFrameTimer;
    private float[] _searchColliderDists;
    private float[] _exitColliderDists;
    private Transform _prevTarget;
    private bool _hasAttached;

    private const float COVERAGE_PER_POS = 22.5f;
    private const float COVERAGE_PER_POS_HALF = 11.25f;
    private const float COVERAGE_TOTAL = 202.5f;
    private const float COVERAGE_HALF = 101.25f;
    private const float MIN_AMT_FOR_TURN = 5.0f;
    private const float EFFECT_X_MULT = 1.6f;
    private const string DIRECT_AIM = "direct";
    private const string SEARCHING = "search";
    private const string TARGETTING = "target";

    private string updateDirectAim()
    {
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
                    aimAtTarget(targetDir);
                    attemptFire(target, targetDir);
                }
            }

            if (!found)
            {
                _cooldownTimer.reset(this.Cooldown / 2);
                _cooldownTimer.start();
            }
        }
        else if (!this.Animator.IsPlaying || _cooldownTimer.Completed)
        {
            _isFiring = false;
        }

        return DIRECT_AIM;
    }

    private void enterSearching()
    {
        enableLight(true);
        _isFiring = false;
        _shotDelayTimer.reset();
        _shotDelayTimer.Paused = true;
        _prevTarget = null;
        float absTargetAngle = Mathf.Min(Vector2.Angle(_normal, _lastAimDir), this.MaxAngle);
        _lastAngle = absTargetAngle * getSign(((Vector2)this.LaunchOrigin.position) + _lastAimDir);
        aimAtAngle();
    }

    private string updateSearching()
    {
        _lastAngle += this.SearchSpeed * _searchDir;
        if (Mathf.Abs(_lastAngle) > this.MaxAngle)
        {
            _lastAngle = Mathf.Sign(_lastAngle) * this.MaxAngle;
            _searchDir = -_searchDir;
        }

        aimAtAngle();
        alignSearchColliders();

        for (int i = 0; i < this.SearchColliders.Length; ++i)
        {
            this.SearchColliders[i].Collide(_inRange, 0, 0, this.DetectionMask);
        }
        
        float dist = float.MaxValue;
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
                    _prevTarget = other;
                    break;
                }
            }
        }

        _inRange.Clear();
        return _prevTarget != null ? TARGETTING : SEARCHING;
    }

    private void enterTargetting()
    {
        SoundManager.Play(this.LockOnSfxKey, this.transform);

        enableLight(false);
        if (_prevTarget != null)
        {
            Vector2 target = this.LaunchOrigin.DirectionTo2D(_prevTarget);
            aimAtTarget(target);
            alignExitColliders();
        }

        _cooldownTimer.reset(this.Cooldown / 2);
        _cooldownTimer.start();
    }

    private string updateTargetting()
    {
        if (_prevTarget == null)
            return SEARCHING;

        Vector2 target = this.LaunchOrigin.DirectionTo2D(_prevTarget);
        bool found = false;

        for (int i = 0; i < this.ExitRangeColliders.Length; ++i)
        {
            if (this.ExitRangeColliders[i].CollideCheck(_prevTarget.gameObject))
            {
                CollisionManager.RaycastResult result = CollisionManager.RaycastFirst((Vector2)this.LaunchOrigin.transform.position, target, this.LaunchOrigin.Distance2D(_prevTarget), this.BlockMask);
                if (!result.Collided || result.Collisions[0].CollidedObject == _prevTarget.gameObject)
                {
                    found = true;
                    break;
                }
            }
        }

        if (!found)
            return SEARCHING;

        if (!_isFiring && !_shotDelayTimer.IsRunning)
        {
            aimAtTarget(target);
            attemptFire(_prevTarget, target);
            alignExitColliders();
        }
        else if (!this.Animator.IsPlaying || _cooldownTimer.Completed)
        {
            _isFiring = false;
        }

        return TARGETTING;
    }

    private void exitTargetting()
    {
        SoundManager.Play(this.UnlockSfxKey, this.transform);
    }

    private void aimAtTarget(Vector2 targetDir)
    {
        _lastAimDir = targetDir;
        float absTargetAngle = Vector2.Angle(_normal, targetDir);
        Vector2 targ = ((Vector2)this.LaunchOrigin.position) + _lastAimDir * 16;
        int sign = getSign(targ);
        float targetAngle = absTargetAngle * sign;

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

        if (this.LightTransform != null)
        {
            //float roundedAngle = Mathf.RoundToInt(absTargetAngle / COVERAGE_PER_POS) * COVERAGE_PER_POS;
            //this.LightTransform.localEulerAngles = new Vector3(0, 0, _currentPos * COVERAGE_PER_POS * -sign);
            this.LightTransform.localEulerAngles = new Vector3(0, 0, absTargetAngle * -sign);
            //this.LightTransform.LookAt(new Vector3(targ.x, targ.y, this.LightTransform.position.z));
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

    private void aimAtAngle()
    {
        aimAtTarget(_normal.VectorAtAngle(_lastAngle));
    }

    private void alignSearchColliders()
    {
        for (int i = 0; i < this.SearchColliders.Length; ++i)
        {
            this.SearchColliders[i].transform.SetPosition2D((IntegerVector)(((Vector2)this.LaunchOrigin.transform.position) + _lastAimDir * _searchColliderDists[i]));
        }
    }

    private void alignExitColliders()
    {
        for (int i = 0; i < this.ExitRangeColliders.Length; ++i)
        {
            this.ExitRangeColliders[i].transform.SetPosition2D((IntegerVector)(((Vector2)this.LaunchOrigin.transform.position) + _lastAimDir * _exitColliderDists[i]));
        }
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
            _cooldownTimer.reset(this.Cooldown);
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
        missile.GetComponent<ExplodeOnCollision>().SetOriginObject(this.gameObject);
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
        SCSpriteAnimation prevAnim = this.Animator.CurrentAnimation;
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
        {
            this.Animator.Stop();

            if (this.Animator.CurrentAnimation != prevAnim)
                SoundManager.Play(this.MoveSfxKey, this.transform);
        }
    }

    private void onDeath()
    {
        PooledObject explosion = this.DeathAnimObject.Retain();
        explosion.transform.SetPosition2D(this.transform.position);
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        this.GetComponent<WorldEntity>().TriggerConsumption(true);
    }
    
    private void freezeFrame(LocalEventNotifier.Event e)
    {
        _freezeFrameTimer.reset((e as FreezeFrameEvent).NumFrames);
    }

    private void enableLight(bool search)
    {
        if (this.SearchLight != null)
            this.SearchLight.enabled = search;
        if (this.TargetLight != null)
            this.TargetLight.enabled = !search;
    }

    private void attachToSurface()
    {
        _hasAttached = true;
        int offsetX = 0;
        int offsetY = 0;

        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                offsetY = -2;
                break;
            case TurretController.AttachDir.Left:
                offsetX = -2;
                break;
            case TurretController.AttachDir.Up:
                offsetY = 2;
                break;
            case TurretController.AttachDir.Right:
                offsetX = 2;
                break;
        }

        GameObject surface = this.integerCollider.CollideFirst(offsetX, offsetY, this.SurfaceLayers);
        if (surface != null)
        {
            this.transform.SetParent(surface.transform);
        }
    }
}
