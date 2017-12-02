using UnityEngine;
using System;
using System.Collections.Generic;

public class SCCharacterController : Actor2D
{
    public enum Facing
    {
        Left = -1,
        Right = 1
    }

    public interface InputWrapper
    {
        int MovementAxis { get; }
        bool JumpBegin { get; }
        bool JumpHeld { get; }
        bool DodgeBegin { get; }
        bool DodgeHeld { get; }
        bool Duck { get; }
        bool LookUp { get; }
        bool AttackLightBegin { get; }
        bool AttackLightHeld { get; }
        bool AttackStrongBegin { get; }
        bool AttackStrongHeld { get; }
        bool UseItem { get; }
        bool Interact { get; }
        bool PausePressed { get; }
    }

    public struct EmptyInput : InputWrapper
    {
        public int MovementAxis { get { return 0; } }
        public bool JumpBegin { get { return false; } }
        public bool JumpHeld { get { return false; } }
        public bool DodgeBegin { get { return false; } }
        public bool DodgeHeld { get { return false; } }
        public bool Duck { get { return false; } }
        public bool LookUp { get { return false; } }
        public bool AttackLightBegin { get { return false; } }
        public bool AttackLightHeld { get { return false; } }
        public bool AttackStrongBegin { get { return false; } }
        public bool AttackStrongHeld { get { return false; } }
        public bool UseItem { get { return false; } }
        public bool Interact { get { return false; } }
        public bool PausePressed { get { return false; } }

        public static EmptyInput Reference = new EmptyInput();
    }

    public LayerMask WallJumpLedgeGrabMask;
    public LayerMask DuckingHaltMovementMask;
    public LayerMask DuckingCollisionMask;
    public LayerMask DeathCollisionMask;
    public LayerMask BounceMask;
    public LayerMask MovingPlatformMask;
    public LayerMask OneWayPlatformMask;

    public float Gravity = 100.0f;
    public float MaxFallSpeed = 500.0f;
    public float FastFallSpeed = 750.0f;
    public float WallSlideSpeed = 100.0f;
    public float InitialWallSlideSpeed = 1.0f;
    public float WallSlideAcceleration = 10.0f;
    public float JumpPower = 320.0f;
    public float JumpHorizontalBoost = 50.0f;
    public float MaxSpeedForJumpHorizontalBoost = 50.0f;
    public float WallJumpYPower = 50.0f;
    public float WallJumpXPower = 50.0f;
    public int WallJumpAutomoveFrames = 4;
    public float JumpHeldGravityMultiplier = 0.5f;
    public float JumpHoldAllowance = 1.0f;
    public int JumpBufferFrames = 6;
    public int JumpGraceFrames = 6;
    public int WallJumpGraceFrames = 4;
    public int AgainstWallCheckOffset = 0;
    public int WallJumpCheckOffset = 0;
    public int LedgeGrabCheckDistance = 6;
    public int LedgeGrabPeekDistance = 8;
    public float LandingHorizontalMultiplier = 0.6f;
    public float Friction = 0.2f;
    public float AirFriction = 0.14f;
    public float MaxRunSpeed = 150f;
    public float RunAcceleration = 15.0f;
    public float RunDecceleration = 3.0f;
    public float AirRunAcceleration = 0.1f;
    public float MinBounceVelocity = 1.0f;
    public float DefaultThrowAngle = 15.0f;
    public float UpwardThrowAngle = 50.0f;
    public float DownwardThrowAngle = -30.0f;
    public float RestingVelocityDecelRate = 10.0f;
    public bool StandupOnSpawn = false;
    public int StandupFrames = 50;
    public bool DisableWallInteractions = false;

    public WorldEntity WorldEntity;
    public Damagable Damagable;
    public AttackController AttackController;
    public IntegerRectCollider Hurtbox;
    public IntegerCollider BlockBox;
    public InventoryController InventoryController;
    public SCMoveSet MoveSet;
    public Transform JumpOriginTransform;
    public Transform NormalJumpOrigin;
    public Transform WallJumpOrigin;
    public Transform LedgeJumpOrigin;
    public Facing CurrentFacing { get { return _facing; } }
    public bool OnGround { get { return _onGround; } }
    public int MoveAxis { get { return _moveAxis; } }
    public SCAttack.HurtboxState HurtboxState = SCAttack.HurtboxState.Normal;
    public bool Ducking { get { return _currentAttack == null && this.HurtboxState == SCAttack.HurtboxState.Ducking; } }
    public bool HitStunned { get { return _freezeFrameTimer.Completed && !_hitStunTimer.Completed; } }
    public InputWrapper MostRecentInput { get; private set; }
    public bool ExecutingMove { get { return _currentAttack != null; } }
    public bool InMoveCooldown { get { return !_cooldownTimer.Completed; } }
    public bool IsWallSliding { get; private set; }
    public bool IsGrabbingLedge { get; private set; }
    public Facing DirectionGrabbingLedge { get; private set; }
    public bool Blocked { get; private set; }
    public bool DidJump { get; private set; }

    public const float DEATH_VELOCITY_MAX = 0.5f;
    public const string LOOT_DROP_EVENT = "LOOT_DROP";
    public const string RESTING_VELOCITY_KEY = "rest";

    public virtual InputWrapper GatherInput()
    {
        return EmptyInput.Reference;
    }

    void Awake()
    {
        _updateFinishEvent = new CharacterUpdateFinishedEvent(null);

        this.localNotifier.Listen(FreezeFrameEvent.NAME, this, freezeFrame);
        this.localNotifier.Listen(HitStunEvent.NAME, this, hitStun);
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
        this.localNotifier.Listen(CancelAttackEvent.NAME, this, onCancelAttack);

        if (this.AttackController != null)
            this.AttackController.HurtboxChangeCallback = attemptHurtboxStateChange;

        _defaultHaltMovementMask = this.HaltMovementMask;
        _defaultCollisionMask = this.CollisionMask;
        _potentialCollisions = new List<IntegerCollider>();
        _potentialWallCollisions = new List<IntegerCollider>();
        _wallJumpExpandAmount = Mathf.Max(Mathf.Max(Mathf.Abs(this.AgainstWallCheckOffset), Mathf.Abs(this.WallJumpCheckOffset)), Mathf.Max(Mathf.Abs(this.LedgeGrabCheckDistance), Mathf.Abs(this.LedgeGrabPeekDistance))) * 2;
        this.Damagable.OnDeathCallback += onDeath;
    }

    public virtual void OnSpawn()
    {
        _hasSpawnedLoot = false;
        _groundedFrames = 0;
        this.Blocked = false;
        this.HurtboxState = SCAttack.HurtboxState.Ducking;
        updateHurtboxForState(this.HurtboxState);
        attemptHurtboxStateChange(SCAttack.HurtboxState.Normal);

        if (_restingVelocityModifier == null)
            _restingVelocityModifier = new VelocityModifier(Vector2.zero, VelocityModifier.CollisionBehavior.sustain);
        else
            _restingVelocityModifier.Modifier = Vector2.zero;
        this.SetVelocityModifier(RESTING_VELOCITY_KEY, _restingVelocityModifier);

        if (_jumpBufferTimer == null)
            _jumpBufferTimer = new Timer(this.JumpBufferFrames);
        _jumpBufferTimer.complete();

        if (_jumpGraceTimer == null)
            _jumpGraceTimer = new Timer(this.JumpGraceFrames);
        _jumpGraceTimer.complete();

        if (_attackTimer == null)
            _attackTimer = new Timer(1);
        _attackTimer.complete();

        if (_freezeFrameTimer == null)
            _freezeFrameTimer = new Timer(1);
        _freezeFrameTimer.complete();

        if (_hitStunTimer == null)
            _hitStunTimer = new Timer(1);
        _hitStunTimer.complete();

        if (_cooldownTimer == null)
            _cooldownTimer = new Timer(1);
        _cooldownTimer.complete();

        if (_comboTimer == null)
            _comboTimer = new Timer(1);
        _comboTimer.complete();

        if (_autoMoveTimer == null)
            _autoMoveTimer = new Timer(1);
        _autoMoveTimer.complete();

        if (this.StandupOnSpawn)
        {
            _hitStunTimer.reset(this.StandupFrames);
            _hitStunTimer.start();
        }

        this.Hurtbox.AddToCollisionPool();
        if (this.BlockBox != null)
            this.BlockBox.AddToCollisionPool();
    }

    public virtual void OnReturnToPool()
    {
        this.Hurtbox.RemoveFromCollisionPool();
        if (this.BlockBox != null)
            this.BlockBox.RemoveFromCollisionPool();
        this.Damagable.ResetLayer();
    }

    public override void FixedUpdate()
    {
        if (!_freezeFrameTimer.Completed)
        {
            _freezeFrameTimer.update();
            if (!_freezeFrameTimer.Completed)
                return;
            else
                this.localNotifier.SendEvent(new FreezeFrameEndedEvent());
        }

        _cooldownTimer.update();
        _comboTimer.update();

        if (!_hitStunTimer.Completed)
        {
            _hitStunTimer.update();

            if (this.Damagable.Dead)
            {
                this.HaltMovementMask = this.DeathCollisionMask;
                this.CollisionMask = this.DeathCollisionMask;

                if (_hitStunTimer.Completed)
                {
                    ObjectPools.Release(this.gameObject);
                    return;
                }
            }
        }

        InputWrapper input = (!_hitStunTimer.Completed || (_currentAttack != null && _currentAttack.LockInput)) ? EmptyInput.Reference : this.GatherInput();
        this.MostRecentInput = input;
        _moveAxis = (_currentAttack == null || !_currentAttack.LockMovement) ? input.MovementAxis : 0;
        _velocity = this.Velocity;
        bool prevOnGround = _onGround;
        GameObject groundedAgainst = this.GroundedAgainst;
        if (groundedAgainst != null)
        {
            _onGround = true;
            ++_groundedFrames;
        }
        else
        {
            _onGround = false;
            _groundedFrames = 0;
        }
        this.DidJump = false;
        this.IsWallSliding = false;
        bool prevGrabbingLedge = this.IsGrabbingLedge && this.TotalVelocity.magnitude < VERY_SMALL_VELOCITY;
        this.IsGrabbingLedge = false;
        bool allowFaceChange = true;
        _autoMoveTimer.update();

        if (_restingVelocityModifier.Modifier.x != 0 || _restingVelocityModifier.Modifier.y != 0)
            _restingVelocityModifier.Modifier = _restingVelocityModifier.Modifier.Approach(this.RestingVelocityDecelRate, Vector2.zero);

        updateControlParameters();

        // Update jumpBufferCounter, and if input indicates Jump is pressed, set it to JUMP_BUFFER
        if (input.JumpBegin)
        {
            _jumpBufferTimer.reset();
            _jumpBufferTimer.start();
        }
        else
        {
            _jumpBufferTimer.update();
        }

        // If we're on ground, do some stuff:
        bool attemptingDrop = false;
        if (_onGround)
        {
            _wallJumpGraceDir = 0;
            if (_jumpGraceTimer.Completed || _jumpGraceTimer.FramesRemaining < _parameters.JumpGraceFrames)
                _jumpGraceTimer.reset(_parameters.JumpGraceFrames);
            if (_jumpGraceTimer.Paused)
                _jumpGraceTimer.start();

            // Check if we're on a moving platform
            int groundedLayerMask = 1 << groundedAgainst.layer;
            if ((groundedLayerMask & this.MovingPlatformMask) == groundedLayerMask)
            {
                attemptMovingPlatformAlignment(groundedAgainst);
            }

            // Check if we're on a one way platform
            else if ((groundedLayerMask & this.OneWayPlatformMask) == groundedLayerMask && GameplayInput.JumpBegin && GameplayInput.Duck)
            {
                attemptingDrop = true;
            }

            // Bounce if necessary
            if ((groundedLayerMask & this.BounceMask) == groundedLayerMask)
            {
                this.DidJump = true;
                if (this.JumpOriginTransform != null)
                    this.JumpOriginTransform.SetLocalPosition2D(this.NormalJumpOrigin.localPosition);

                _velocity.y = Mathf.Min(this.MinBounceVelocity, (this.MinBounceVelocity + Mathf.Abs(_velocity.y)) / 2.0f);
                _velocity.x += (int)_facing * this.MinBounceVelocity * HORIZ_BOUNCE_FACTOR;
            }
        }

        // Otherwise, update our jump grace counter (for stored jumps)
        else
        {
            _jumpGraceTimer.update();
            if (_wallJumpGraceDir != 0 && _jumpGraceTimer.Completed)
                _wallJumpGraceDir = 0;
        }

        // If we're auto-moving, do that
        if (!_autoMoveTimer.Completed)
        {
            if ((_velocity.x < 0 && _autoMoveValue.x < 0) || (_velocity.x > 0 && _autoMoveValue.x > 0))
                _velocity.x = Mathf.Sign(_autoMoveValue.x) * Mathf.Max(Mathf.Abs(_autoMoveValue.x), Mathf.Abs(_velocity.x));
            else
                _velocity.x = _autoMoveValue.x;

            if ((_velocity.y < 0 && _autoMoveValue.y < 0) || (_velocity.y > 0 && _autoMoveValue.y > 0))
                _velocity.y = Mathf.Sign(_autoMoveValue.y) * Mathf.Max(Mathf.Abs(_autoMoveValue.y), Mathf.Abs(_velocity.y));
            else
                _velocity.y = _autoMoveValue.y;

            allowFaceChange = false;
            _facing = (Facing)Mathf.RoundToInt(Mathf.Sign(_velocity.x));
        }

        // Friction (No friction if continue to hold in direction of movement in air)
        else if (_moveAxis != Math.Sign(_velocity.x) || (_onGround && _moveAxis == 0))
        {
            float maxSlowDown = _onGround ? _parameters.Friction : _parameters.AirFriction;
            _velocity.x = _velocity.x.Approach(0.0f, maxSlowDown);
        }

        // Normal movement
        if (_autoMoveTimer.Completed && _moveAxis != 0)
        {
            // Landing speed multiplier
            if (_onGround && !prevOnGround)
                _velocity.x *= this.LandingHorizontalMultiplier;

            // Deccel if past max speed
            if (Mathf.Abs(_velocity.x) > _parameters.MaxRunSpeed && Math.Sign(_velocity.x) == _moveAxis)
            {
                _velocity.x = _velocity.x.Approach(_parameters.MaxRunSpeed * _moveAxis, _parameters.RunDecceleration);
            }

            // Accelerate
            else
            {
                float acceleration = _onGround ? _parameters.RunAcceleration : _parameters.AirRunAcceleration;
                _velocity.x = _velocity.x.Approach(_parameters.MaxRunSpeed * _moveAxis, acceleration);
            }
        }

        // Handle falling
        bool againstWall = false;
        bool leftWallJumpValid = false;
        bool rightWallJumpValid = false;
        Facing dir = prevGrabbingLedge ? this.DirectionGrabbingLedge : (Facing)_moveAxis;
        if (!_onGround)
        {
            againstWall = prevGrabbingLedge || checkAgainstWall((Facing)_moveAxis);

            if (_autoMoveTimer.Completed)
            {
                // If jump button is held down use smaller number for gravity
                float gravity = (input.JumpHeld && _canJumpHold && (Math.Sign(_velocity.y) == 1 || Mathf.Abs(_velocity.y) < _parameters.JumpHoldAllowance)) ? (_parameters.JumpHeldGravityMultiplier * _parameters.Gravity) : _parameters.Gravity;
                float targetFallSpeed = _parameters.MaxFallSpeed;

                // Check if we're wall sliding
                if (_currentAttack == null && _velocity.y <= 0.0f && !input.JumpBegin && !input.Duck && againstWall)
                {
                    ++_wallSlideTime;
                    targetFallSpeed = Mathf.Lerp(this.InitialWallSlideSpeed, this.WallSlideSpeed, (float)_wallSlideTime / (float)this.WallSlideAcceleration);
                    this.IsWallSliding = true;
                }

                // Check if we need to fast fall
                else if (input.Duck && Math.Sign(_velocity.y) == -1)
                    targetFallSpeed = _parameters.FastFallSpeed;

                _velocity.y = _velocity.y.Approach(-targetFallSpeed, gravity);
            }

            leftWallJumpValid = (againstWall && dir == Facing.Left) || (!againstWall && checkAgainstWall(Facing.Left));
            rightWallJumpValid = !leftWallJumpValid && (againstWall || checkAgainstWall(Facing.Right));
        }

        if (this.IsWallSliding)
            ++_wallSlideTime;
        else if (_wallSlideTime > 0)
            _wallSlideTime = 0;

        // Determine if wall jumping, normal jumping, or ledge grabbing
        bool wallJumpValid = leftWallJumpValid || rightWallJumpValid;
        if (wallJumpValid)
        {
            _jumpGraceTimer.reset(this.WallJumpGraceFrames);
            _jumpGraceTimer.start();
            _wallJumpGraceDir = leftWallJumpValid ? -1 : 1;
        }
        else
        {
            wallJumpValid = _wallJumpGraceDir != 0 && !_jumpGraceTimer.Completed;
            if (wallJumpValid)
            {
                leftWallJumpValid = _wallJumpGraceDir == -1;
                rightWallJumpValid = !leftWallJumpValid;
            }
        }

        bool attemptingJump = !attemptingDrop && checkForJump(input, _onGround, wallJumpValid);
        bool ledgeGrabbing = false;
        if (!_onGround && !input.Duck && (_velocity.y < 0.0f || prevGrabbingLedge))
        {
            if (leftWallJumpValid && dir == Facing.Left)
                ledgeGrabbing = checkTopHalfLedgeGrab(Facing.Left, prevGrabbingLedge);
            else if (rightWallJumpValid && dir == Facing.Right)
                ledgeGrabbing = checkTopHalfLedgeGrab(Facing.Right, prevGrabbingLedge);
            if (ledgeGrabbing)
                wallJumpValid = false;
        }
        
        // Check for interrupts
        SCAttack interruptingMove = null;
        if (_currentAttack != null)
        {
            bool interrupted = false;
            if ((attemptingJump || attemptingDrop) && canJumpInterrupt())
            {
                interrupted = true; // Jump, Wall Jump, or platform drop interrupt
            }
            else if (canMoveInterrupt())
            {
                interruptingMove = this.MoveSet.GetAttackForInput(input, this, _currentAttack.MoveInterruptCategoryMask);
                if (interruptingMove != null)
                    interrupted = true; // Move interrupt
            }

            if (ledgeGrabbing)
            {
                if (!interrupted && canLedgeGrabInterrupt())
                    interrupted = true; // Ledge grab interrupt
                else
                    ledgeGrabbing = false;
            }

            if (!interrupted && _onGround && _currentAttack.GroundedEffect != SCAttack.OnGroundEffect.None && _groundedFrames >= _currentAttack.GroundedFramesForEffect)
            {
                interrupted = true; // Interrupt into grounded combo for this move
                if (_currentAttack.GroundedEffect == SCAttack.OnGroundEffect.Combo)
                    interruptingMove = _currentAttack.GroundedCombo;
            }

            if (interrupted)
            {
                _attackTimer.complete();
                endMove();
            }
        }

        if (_currentAttack == null || interruptingMove != null)
        {
            // Attempt to stand up if necessary
            attemptHurtboxStateChange(SCAttack.HurtboxState.Normal);

            // Check if we're using an item
            if (interruptingMove == null && input.UseItem && this.InventoryController.NumItems > 0)
            {
                useItem(input);
            }

            // Check if it's time to jump or drop through a platform
            else if (interruptingMove == null && (attemptingJump || attemptingDrop))
            {
                if (attemptingDrop)
                {
                    drop();
                }
                else if (wallJumpValid)
                {
                    if (leftWallJumpValid && checkAgainstWallForWallJump(Facing.Left))
                        wallJump(Facing.Right);
                    else if (checkAgainstWallForWallJump(Facing.Right))
                        wallJump(Facing.Left);
                }
                else
                {
                    if (this.JumpOriginTransform != null)
                            this.JumpOriginTransform.SetLocalPosition2D(prevGrabbingLedge ? this.LedgeJumpOrigin.localPosition : this.NormalJumpOrigin.localPosition);
                    jump();
                }
            }

            // Or if we're beginning a Move
            else
            {
                // Check for combo window or buffer first
                bool comboing = false;
                if (interruptingMove == null && (_comboBufferInput != SCMoveSet.MoveInput.None || !_comboTimer.Completed))
                {
                    _currentAttack = this.MoveSet.GetComboMove(input, _comboSource, _comboBufferInput);
                    comboing = _currentAttack != null;
                }
                if (!comboing)
                    _currentAttack = interruptingMove != null ? interruptingMove : this.MoveSet.GetAttackForInput(input, this);

                // Begin the move, if valid
                if (_currentAttack != null)
                {
                    if (!comboing && !_cooldownTimer.Completed && (((int)_currentAttack.Category & _cooldownCategoryMask) != 0))
                    {
                        _currentAttack = null;
                    }
                    else
                    {
                        _cooldownTimer.complete();
                        _comboTimer.complete();
                        _comboBufferInput = SCMoveSet.MoveInput.None;
                        _attackTimer.reset(_currentAttack.NormalFrameLength);
                        _attackTimer.start();
                        _autoMoveTimer.complete();

                        // Change direction facing if necessary (need to do this before checking first frame velocity boost)
                        if (_moveAxis != 0)
                            _facing = (Facing)_moveAxis;

                        // Apply velocity boost for current frame in current Move
                        handleVelocityBoosts();
                        allowFaceChange = false;
                    }
                }

                // Otherwise, might be ledge grabbing
                else if (ledgeGrabbing)
                {
                    grabLedge((Facing)_moveAxis, prevGrabbingLedge);
                    this.IsGrabbingLedge = true;
                }
            }
        }
        else
        {
            // See if we should buffer a combo
            if (_currentAttack.Combos != null && _currentAttack.Combos.Length > 0 && _currentAttack.ComboBuffer >= _attackTimer.FramesRemaining)
            {
                SCMoveSet.MoveInput comboInput = SCMoveSet.GetCurrentMoveInput(input);
                if (comboInput != SCMoveSet.MoveInput.None)
                    _comboBufferInput = comboInput;
            }

            // Update the Move timer, check if done
            _attackTimer.update();
            if (_attackTimer.Completed)
            {
                // End Move, and stand up if necessary
                endMove();
                attemptHurtboxStateChange(SCAttack.HurtboxState.Normal);
            }
            else
            {
                // Apply velocity boost for current frame in current Move
                allowFaceChange = handleVelocityBoosts();
            }
        }

        // Change direction facing if necessary
        if (allowFaceChange && _moveAxis != 0)
            _facing = (Facing)_moveAxis;

        // Actor2D update
        this.Velocity = _velocity;
        base.FixedUpdate();

        // Check if need to drop loot
        if (this.Damagable.Dead && !_hasSpawnedLoot && this.Velocity.x < DEATH_VELOCITY_MAX && this.GroundedAgainst != null)
        {
            _hasSpawnedLoot = true;
            this.localNotifier.SendEvent(new LocalEventNotifier.Event(LOOT_DROP_EVENT));
        }

        // Send update finished event (so visual state handling can know to update)
        _updateFinishEvent.CurrentAttack = _currentAttack;
        this.localNotifier.SendEvent(_updateFinishEvent);
        _hasGatheredPotentialCollisions = false;

        // Update block box (only blocking when not attacking, on ground, and not hitstunned
        //TODO: Also dependent on block input, which guard AI should just always have on
        if (this.BlockBox != null)
            this.BlockBox.enabled = _onGround && (_hitStunTimer.Completed || this.Blocked) && (this.AttackController == null || this.AttackController.CanBlock(_currentAttack));

        // Update Move hitboxes
        if (this.AttackController != null)
            this.AttackController.UpdateHitBoxes(_currentAttack, this.HurtboxState, _facing);
    }

    public GameObject GroundedAgainst
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.HaltMovementMask, null, potentialCollisions()); }
    }

    // Use carefully and sparingly, probably only before first update call
    public void SetFacingDirectly(Facing facing)
    {
        _facing = facing;
    }

    /**
     * Private
     */
    private LayerMask _defaultHaltMovementMask;
    private LayerMask _defaultCollisionMask;
    private Facing _facing;
    private Vector2 _velocity;
    private Timer _jumpBufferTimer;
    private Timer _jumpGraceTimer;
    private int _moveAxis;
    private bool _canJumpHold;
    private bool _onGround;
    private SCAttack _currentAttack = null;
    private CharacterUpdateFinishedEvent _updateFinishEvent;
    private Timer _attackTimer;
    private ControlParameters _parameters;
    private Timer _freezeFrameTimer;
    private Timer _hitStunTimer;
    private float _hitStunGravityMultiplier;
    private float _hitStunAirFrictionMultiplier;
    private Timer _cooldownTimer;
    private Timer _comboTimer;
    private SCMoveSet.MoveInput _comboBufferInput;
    private SCAttack _comboSource;
    private int _cooldownCategoryMask;
    private Timer _autoMoveTimer;
    private Vector2 _autoMoveValue;
    private int _ledgeGrabY;
    private bool _hasSpawnedLoot;
    private int _wallSlideTime;
    private VelocityModifier _restingVelocityModifier;
    private List<IntegerCollider> _potentialCollisions;
    private List<IntegerCollider> _potentialWallCollisions;
    private bool _hasGatheredPotentialCollisions;
    private int _wallJumpExpandAmount;
    private int _wallJumpGraceDir;
    private int _groundedFrames;

    private const float HORIZ_BOUNCE_FACTOR = 0.8f;
    private const float VERY_SMALL_VELOCITY = 0.02f;

    private struct ControlParameters
    {
        public float Gravity;
        public float MaxFallSpeed;
        public float FastFallSpeed;
        public float JumpPower;
        public float JumpHorizontalBoost;
        public float JumpHeldGravityMultiplier;
        public float JumpHoldAllowance;
        public int JumpBufferFrames;
        public int JumpGraceFrames;
        public float LandingHorizontalMultiplier;
        public float Friction;
        public float AirFriction;
        public float MaxRunSpeed;
        public float RunAcceleration;
        public float RunDecceleration;
        public float AirRunAcceleration;

        public Vector2 VelocityBoost;
        public SCAttack.VelocityBoost.BoostType VelocityBoostType;
    }

    private void updateControlParameters()
    {
        if (_currentAttack != null)
        {
            // Update with parameters for current attack
            _parameters.Gravity = this.Gravity * _currentAttack.GravityMultiplier;
            _parameters.JumpPower = this.JumpPower * _currentAttack.JumpPowerMultiplier;
            _parameters.JumpHorizontalBoost = this.JumpHorizontalBoost * _currentAttack.JumpHorizontalBoostMultiplier;
            _parameters.Friction = this.Friction * _currentAttack.FrictionMultiplier;
            _parameters.AirFriction = this.AirFriction * _currentAttack.AirFrictionMultiplier;
            _parameters.MaxRunSpeed = this.MaxRunSpeed * _currentAttack.MaxRunSpeedMultiplier;
            _parameters.RunAcceleration = this.RunAcceleration * _currentAttack.RunAccelerationMultiplier;
            _parameters.RunDecceleration = this.RunDecceleration * _currentAttack.RunDeccelerationMultiplier;
            _parameters.AirRunAcceleration = this.AirRunAcceleration * _currentAttack.AirRunAccelerationMultiplier;
            _parameters.VelocityBoostType = SCAttack.VelocityBoost.BoostType.None;

            if (this.AttackController != null)
            {
                SCAttack.VelocityBoost? boost = this.AttackController.GetCurrentVelocityBoost(_currentAttack);
                if (boost.HasValue)
                {
                    _parameters.VelocityBoost = boost.Value.Boost;
                    _parameters.VelocityBoostType = boost.Value.Type;
                }
            }
        }
        else
        {
            // Otherwise use standard parameters
            _parameters.Gravity = this.Gravity;
            _parameters.JumpPower = this.JumpPower;
            _parameters.JumpHorizontalBoost = this.JumpHorizontalBoost;
            _parameters.Friction = this.Friction;
            _parameters.AirFriction = this.AirFriction;
            _parameters.MaxRunSpeed = this.MaxRunSpeed;
            _parameters.RunAcceleration = this.RunAcceleration;
            _parameters.RunDecceleration = this.RunDecceleration;
            _parameters.AirRunAcceleration = this.AirRunAcceleration;
            _parameters.VelocityBoostType = SCAttack.VelocityBoost.BoostType.None;

            // Check if we need to apply hit stun multipliers
            if (!_hitStunTimer.Completed)
            {
                _parameters.Gravity *= _hitStunGravityMultiplier;
                _parameters.AirFriction *= _hitStunAirFrictionMultiplier;
            }
        }

        _parameters.MaxFallSpeed = this.MaxFallSpeed;
        _parameters.FastFallSpeed = this.FastFallSpeed;
        _parameters.JumpHeldGravityMultiplier = this.JumpHeldGravityMultiplier;
        _parameters.JumpHoldAllowance = this.JumpHoldAllowance;
        _parameters.JumpBufferFrames = this.JumpBufferFrames;
        _parameters.JumpGraceFrames = this.JumpGraceFrames;
        _parameters.LandingHorizontalMultiplier = this.LandingHorizontalMultiplier;
    }

    private List<IntegerCollider> potentialCollisions()
    {
        if (!_hasGatheredPotentialCollisions)
            gatherPotentialCollisions();
        return _potentialCollisions;
    }

    private List<IntegerCollider> potentialWallCollisions()
    {
        if (!_hasGatheredPotentialCollisions)
            gatherPotentialCollisions();
        return _potentialWallCollisions;
    }

    private void gatherPotentialCollisions()
    {
        this.integerCollider.GetPotentialCollisions(0, 0, 0, 0, this.HaltMovementMask, _potentialCollisions, 2, 2);
        this.integerCollider.GetPotentialCollisions(0, 0, 0, 0, this.WallJumpLedgeGrabMask, _potentialWallCollisions, _wallJumpExpandAmount, _wallJumpExpandAmount);
        _hasGatheredPotentialCollisions = true;
    }

    private void onCollide(LocalEventNotifier.Event e)
    {
        _autoMoveTimer.complete();

        //NOTE: This isn't necessary without vertical moving platforms
        /*List<GameObject> collisions = (e as CollisionEvent).Hits;
        for (int i = 0; i < collisions.Count; ++i)
        {
            int collidedLayer = 1 << collisions[i].layer;
            if ((collidedLayer & this.MovingPlatformMask) == collidedLayer)
            {
                if (attemptMovingPlatformAlignment(collisions[i]))
                    break;
            }
        }*/
    }

    private bool attemptMovingPlatformAlignment(GameObject platform)
    {
        IMovingPlatform movingPlatform = platform.GetComponent<IMovingPlatform>();
        if (movingPlatform != null)
        {
            _restingVelocityModifier.Modifier = movingPlatform.Velocity;
            return true;
        }
        return false;
    }

    private bool checkAgainstWall(Facing direction)
    {
        if (this.DisableWallInteractions)
            return false;

        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + 1 + this.Hurtbox.Offset.Y - this.Hurtbox.Bounds.Size.Y / 2 - this.AgainstWallCheckOffset);
        bool retVal = this.CollisionManager.CollidePointFirst(checkPoint, potentialWallCollisions()) != null;

        //Debug
        Debug.DrawLine(new Vector3(this.transform.position.x + ((int)direction) * this.Hurtbox.Offset.X, checkPoint.Y, -5), new Vector3(checkPoint.X, checkPoint.Y, -5), retVal ? Color.blue : Color.red, 0.1f);
        return retVal;
    }

    private bool checkAgainstWallForWallJump(Facing direction)
    {
        if (this.DisableWallInteractions)
            return false;

        if (this.WallJumpCheckOffset == this.AgainstWallCheckOffset)
            return true;

        if (_wallJumpGraceDir == (int)direction && !_jumpGraceTimer.Completed)
            return true;

        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + 1 + this.Hurtbox.Offset.Y - this.Hurtbox.Bounds.Size.Y / 2 - this.WallJumpCheckOffset);
        return this.CollisionManager.CollidePointFirst(checkPoint, potentialWallCollisions()) != null;
    }

    private bool checkTopHalfLedgeGrab(Facing direction, bool prevGrabbingLedge)
    {
        if (this.DisableWallInteractions)
            return false;

        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + this.Hurtbox.Offset.Y + this.Hurtbox.Bounds.Size.Y / 2 - this.LedgeGrabCheckDistance);
        return this.CollisionManager.CollidePointFirst(checkPoint, potentialWallCollisions()) == null && checkLedgeGrab(direction, prevGrabbingLedge);
    }

    private bool checkLedgeGrab(Facing direction, bool prevGrabbingLedge)
    {
        if (this.DisableWallInteractions)
            return false;

        if (prevGrabbingLedge)
            return true;

        IntegerVector rayOrigin = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + this.Hurtbox.Offset.Y + this.Hurtbox.Bounds.Size.Y / 2 - LedgeGrabCheckDistance);
        CollisionManager.RaycastResult result = this.CollisionManager.RaycastFirst(rayOrigin, Vector2.down, this.Hurtbox.Bounds.Size.Y - this.LedgeGrabCheckDistance + 2, this.WallJumpLedgeGrabMask, null, potentialWallCollisions());
        IntegerVector ledgeTop = result.FarthestPointReached;
        _ledgeGrabY = ledgeTop.Y - this.Hurtbox.Offset.Y - this.Hurtbox.Size.Y / 2 + this.LedgeGrabPeekDistance;
        bool retVal = this.Hurtbox.CollideFirst(0, _ledgeGrabY - Mathf.RoundToInt(this.transform.position.y), this.HaltMovementMask, null, null) == null;

        //Debug
        Debug.DrawLine(new Vector3(rayOrigin.X, rayOrigin.Y, -5), new Vector3(rayOrigin.X, rayOrigin.Y - (this.Hurtbox.Bounds.Size.Y - this.LedgeGrabCheckDistance + 2), -5), retVal ? Color.blue : Color.red, 10.0f);
        Debug.DrawLine(new Vector3(rayOrigin.X + (int)direction, rayOrigin.Y, -5), new Vector3(result.FarthestPointReached.X + (int)direction, result.FarthestPointReached.Y, -5), Color.green, 10.0f);
        return retVal;
    }

    private void grabLedge(Facing direction, bool prevGrabbingLedge)
    {
        if (!prevGrabbingLedge)
        {
            this.transform.SetY(_ledgeGrabY);
            this.Move(new IntegerVector((int)direction * 2, 0), null);
            this.DirectionGrabbingLedge = direction;
        }
        _velocity.x = 0;
        _velocity.y = 0;
    }

    private bool checkForJump(InputWrapper input, bool onGround, bool againstWall)
    {
        return (input.JumpBegin || !_jumpBufferTimer.Completed) && (onGround || againstWall || !_jumpGraceTimer.Completed);
    }

    private bool canJumpInterrupt()
    {
        return _currentAttack.NormalFrameLength - _attackTimer.FramesRemaining >= _currentAttack.JumpInterruptFrame;
    }

    private bool canMoveInterrupt()
    {
        return _currentAttack.NormalFrameLength - _attackTimer.FramesRemaining >= _currentAttack.MoveInterruptFrame;
    }

    private bool canLedgeGrabInterrupt()
    {
        return _currentAttack.NormalFrameLength - _attackTimer.FramesRemaining >= _currentAttack.LedgeGrabInterruptFrame;
    }

    private void endMove()
    {
        if (_currentAttack.CooldownDuration > 0)
        {
            _cooldownTimer.reset(_currentAttack.CooldownDuration);
            _cooldownCategoryMask = _currentAttack.CooldownCategoriesMask;
        }
        if (_currentAttack.ComboWindow > 0)
        {
            _comboTimer.reset(_currentAttack.ComboWindow);
            _comboSource = _currentAttack;
        }
        _currentAttack = null;
    }

    private void jump()
    {
        this.DidJump = true;
        _jumpBufferTimer.complete();
        _jumpGraceTimer.complete();

        _velocity.y = this.JumpPower;

        if (_moveAxis != 0)
        {
            if (Mathf.RoundToInt(Mathf.Sign(_velocity.x)) == _moveAxis)
            {
                _velocity.x = _velocity.x.Approach(_moveAxis * this.MaxSpeedForJumpHorizontalBoost, _parameters.JumpHorizontalBoost);
            }
            else
            {
                _velocity.x += _parameters.JumpHorizontalBoost * _moveAxis;
            }
        }

        _canJumpHold = true;
    }

    private void drop()
    {
        this.HaltMovementMask = this.HaltMovementMask & ~this.OneWayPlatformMask;
        this.CollisionMask = this.CollisionMask & ~this.OneWayPlatformMask;
        base.Move(new Vector2(0, -2));
        this.HaltMovementMask = this.HaltMovementMask | this.OneWayPlatformMask;
        this.CollisionMask = this.CollisionMask | this.OneWayPlatformMask;

        _jumpBufferTimer.complete();
        _jumpGraceTimer.complete();
    }

    private void wallJump(Facing direction)
    {
        this.DidJump = true;
        if (this.JumpOriginTransform != null)
            this.JumpOriginTransform.SetLocalPosition2D(this.WallJumpOrigin.localPosition);

        _jumpBufferTimer.complete();
        _jumpGraceTimer.complete();

        _autoMoveTimer.reset(this.WallJumpAutomoveFrames);
        _autoMoveValue.x = ((int)direction) * this.WallJumpXPower;
        _autoMoveValue.y = this.WallJumpYPower;
        _velocity.x = _autoMoveValue.x;
        _velocity.y = Mathf.Max(_autoMoveValue.y, _velocity.y);
        _canJumpHold = true;
    }

    private void freezeFrame(LocalEventNotifier.Event e)
    {
        _freezeFrameTimer.reset((e as FreezeFrameEvent).NumFrames);
    }

    private void hitStun(LocalEventNotifier.Event e)
    {
        HitStunEvent stunEvent = e as HitStunEvent;
        if (!stunEvent.Raging)
        {
            _currentAttack = null;
            this.Blocked = stunEvent.Blocked;
            _attackTimer.complete();
            _hitStunGravityMultiplier = stunEvent.GravityMultiplier;
            _hitStunAirFrictionMultiplier = stunEvent.AirFrictionMultiplier;
            _hitStunTimer.reset(stunEvent.NumFrames);
        }
    }

    private bool attemptHurtboxStateChange(SCAttack.HurtboxState newState)
    {
        if (newState == this.HurtboxState)
            return true;
        updateHurtboxForState(newState);
        if (this.Hurtbox.CollideFirst(0, 0, this.HaltMovementMask, null, potentialCollisions()) != null)
        {
            // We collided when trying to stand up, so boot out of attack into ducking position
            _currentAttack = null;
            updateHurtboxForState(this.HurtboxState);
            return false;
        }

        this.HurtboxState = newState;
        return true;
    }

    private void updateHurtboxForState(SCAttack.HurtboxState state)
    {
        _hasGatheredPotentialCollisions = false;
        switch (state)
        {
            default:
            case SCAttack.HurtboxState.Normal:
                this.Hurtbox.Offset = this.MoveSet.NormalHitboxSpecs.Center;
                this.Hurtbox.Size = this.MoveSet.NormalHitboxSpecs.Size;
                this.HaltMovementMask = _defaultHaltMovementMask;
                this.CollisionMask = _defaultCollisionMask;
                break;
            case SCAttack.HurtboxState.Ducking:
                this.Hurtbox.Offset = this.MoveSet.DuckHitboxSpecs.Center;
                this.Hurtbox.Size = this.MoveSet.DuckHitboxSpecs.Size;
                this.HaltMovementMask = this.DuckingHaltMovementMask;
                this.CollisionMask = this.DuckingCollisionMask;
                break;
        }
    }

    private bool handleVelocityBoosts()
    {
        Vector2 boost = _parameters.VelocityBoost;
        boost.x *= (int)_facing;
        switch (_parameters.VelocityBoostType)
        {
            default:
            case SCAttack.VelocityBoost.BoostType.None:
                return true;
            case SCAttack.VelocityBoost.BoostType.Additive:
                _velocity += boost;
                break;
            case SCAttack.VelocityBoost.BoostType.Average:
                _velocity = (_velocity + boost) / 2.0f;
                break;
            case SCAttack.VelocityBoost.BoostType.Absolute:
                _velocity = boost;
                break;
            case SCAttack.VelocityBoost.BoostType.AverageXOnly:
                _velocity.x = (boost.x + _velocity.x) / 2.0f;
                break;
        }
        return boost.x == 0.0f;
    }
    
    private void useItem(InputWrapper input)
    {
        PooledObject item = this.InventoryController.UseItem(0);
        if (item != null)
        {
            item.transform.SetPosition2D(this.transform.position.x, this.transform.position.y);
            ThrownActor actor = item.GetComponent<ThrownActor>();
            if (actor != null)
            {
                //TODO: Throw origin position marked in character data
                float angle = this.DefaultThrowAngle;
                if (input.Duck)
                    angle = this.DownwardThrowAngle;
                else if (input.LookUp)
                    angle = this.UpwardThrowAngle;
                actor.Throw(angle, (int)_facing);
            }

            item.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void onCancelAttack(LocalEventNotifier.Event e)
    {
        _attackTimer.complete();
        endMove();
    }

    private void onDeath()
    {
        this.WorldEntity.TriggerConsumption(false);
    }
}
