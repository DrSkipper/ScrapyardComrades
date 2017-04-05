using UnityEngine;
using System;
using System.Collections.Generic;

public class SCCharacterController : Actor2D, ISpawnable
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
        public bool AttackLightBegin { get { return false; } }
        public bool AttackLightHeld { get { return false; } }
        public bool AttackStrongBegin { get { return false; } }
        public bool AttackStrongHeld { get { return false; } }
        public bool UseItem { get { return false; } }
        public bool Interact { get { return false; } }
        public bool PausePressed { get { return false; } }

        public static EmptyInput Reference = new EmptyInput();
    }

    public float Gravity = 100.0f;
    public float MaxFallSpeed = 500.0f;
    public float FastFallSpeed = 750.0f;
    public float WallSlideSpeed = 100.0f;
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

    public WorldEntity WorldEntity;
    public Damagable Damagable;
    public AttackController AttackController;
    public IntegerRectCollider Hurtbox;
    public InventoryController InventoryController;
    public SCMoveSet MoveSet;
    public Facing CurrentFacing { get { return _facing; } }
    public bool OnGround { get { return _onGround; } }
    public int MoveAxis { get { return _moveAxis; } }
    public SCAttack.HurtboxState HurtboxState = SCAttack.HurtboxState.Normal;
    public bool Ducking { get { return _currentAttack == null && this.HurtboxState == SCAttack.HurtboxState.Ducking; } }
    public bool HitStunned { get { return _freezeFrameTimer.Completed && !_hitStunTimer.Completed; } }
    public InputWrapper MostRecentInput { get; private set; }
    public bool ExecutingMove { get { return _currentAttack == null; } }
    public bool IsWallSliding { get; private set; }
    public bool IsGrabbingLedge { get; private set; }
    public Facing DirectionGrabbingLedge { get; private set; }

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

        if (this.AttackController != null)
            this.AttackController.HurtboxChangeCallback = attemptHurtboxStateChange;
    }

    public virtual void OnSpawn()
    {
        this.HurtboxState = SCAttack.HurtboxState.Normal;
        updateHurtboxForState(this.HurtboxState);

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

        if (_autoMoveTimer == null)
            _autoMoveTimer = new Timer(1);
        _autoMoveTimer.complete();

        //TODO - Data-drive health
        this.Damagable.Health = 10;

        this.Hurtbox.AddToCollisionPool();
        if (this.AttackController != null)
            this.AttackController.AddDamageBoxes();
    }

    void OnReturnToPool()
    {
        this.Hurtbox.RemoveFromCollisionPool();
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
        
        if (!_hitStunTimer.Completed)
        {
            _hitStunTimer.update();

            if (_hitStunTimer.Completed && this.Damagable.Dead)
            {
                this.WorldEntity.TriggerConsumption();
                return;
            }
        }

        InputWrapper input = (!_hitStunTimer.Completed || (_currentAttack != null && _currentAttack.LockInput)) ? EmptyInput.Reference : this.GatherInput();
        this.MostRecentInput = input;
        _moveAxis = input.MovementAxis;
        _velocity = this.Velocity;
        bool prevOnGround = _onGround;
        _onGround = this.IsGrounded;
        this.IsWallSliding = false;
        bool prevGrabbingLedge = this.IsGrabbingLedge;
        this.IsGrabbingLedge = false;
        bool allowFaceChange = true;
        _autoMoveTimer.update();

        updateControlParameters();

        // Update jumpBufferCounter, and if input indicates Jump is pressed, set it to JUMP_BUFFER
        _jumpBufferTimer.update();
        if (input.JumpBegin)
        {
            _jumpBufferTimer.reset();
            _jumpBufferTimer.start();
        }

        // If we're on ground, do some stuff:
        if (_onGround)
        {
            if (_jumpGraceTimer.Completed || _jumpGraceTimer.FramesRemaining < _parameters.JumpGraceFrames)
                _jumpGraceTimer.reset(_parameters.JumpGraceFrames);
            if (_jumpGraceTimer.Paused)
                _jumpGraceTimer.start();
        }

        // Otherwise, update our jump grace counter (for stored jumps)
        else
        {
            _jumpGraceTimer.update();
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
                    targetFallSpeed = this.WallSlideSpeed;
                    this.IsWallSliding = true;
                }

                // Check if we need to fast fall
                else if (input.Duck && Math.Sign(_velocity.y) == -1)
                    targetFallSpeed = _parameters.FastFallSpeed;

                _velocity.y = _velocity.y.Approach(-targetFallSpeed, -gravity);
            }

            leftWallJumpValid = (againstWall && (Facing)_moveAxis == Facing.Left) || (!againstWall && checkAgainstWall(Facing.Left));
            rightWallJumpValid = !leftWallJumpValid && (againstWall || checkAgainstWall(Facing.Right));
        }

        // Determine if wall jumping, normal jumping, or ledge grabbing
        bool wallJumpValid = leftWallJumpValid || rightWallJumpValid;
        bool attemptingJump = checkForJump(input, _onGround, wallJumpValid);
        bool ledgeGrabbing = false;
        if (!_onGround && !input.Duck && _velocity.y < 0.0f)
        {
            if (leftWallJumpValid && (Facing)_moveAxis == Facing.Left)
                ledgeGrabbing = checkTopHalfLedgeGrab(Facing.Left, prevGrabbingLedge);
            else if (rightWallJumpValid && (Facing)_moveAxis == Facing.Right)
                ledgeGrabbing = checkTopHalfLedgeGrab(Facing.Right, prevGrabbingLedge);
            if (ledgeGrabbing)
                wallJumpValid = false;
        }
        
        // Check for interrupts
        SCAttack interruptingMove = null;
        if (_currentAttack != null)
        {
            bool interrupted = false;
            if (attemptingJump && canJumpInterrupt())
            {
                interrupted = true; // Jump or Wall Jump interrupt
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

            if (interrupted)
            {
                _attackTimer.complete();
                endMove();
            }
        }

        if (_currentAttack == null)
        {
            // Attempt to stand up if necessary
            attemptHurtboxStateChange(SCAttack.HurtboxState.Normal);

            // Check if we're using an item
            if (input.UseItem && this.InventoryController.NumItems > 0)
            {
                PooledObject item = this.InventoryController.UseItem(0);
                if (item != null)
                {
                    item.transform.SetPosition2D(this.transform.position.x, this.transform.position.y);
                    ThrownActor actor = item.GetComponent<ThrownActor>();
                    if (actor != null)
                    {
                        //TODO: Throw position marked in character data
                        //TODO: Item thrown up a bit if looking up
                        actor.Throw(item.GetComponent<Pickup>().Data.ThrowVelocity * new Vector2((int)_facing, 0));
                    }

                    ISpawnable[] spawnables = item.GetComponents<ISpawnable>();

                    for (int i = 0; i < spawnables.Length; ++i)
                    {
                        spawnables[i].OnSpawn();
                    }
                }
            }

            // Check if it's time to jump
            else if (attemptingJump)
            {
                if (wallJumpValid)
                {
                    if (leftWallJumpValid && checkAgainstWallForWallJump(Facing.Left))
                        wallJump(Facing.Right);
                    else if (checkAgainstWallForWallJump(Facing.Right))
                        wallJump(Facing.Left);
                }
                else
                {
                    jump();
                }
            }

            // Or if we're beginning a Move
            else
            {
                _currentAttack = interruptingMove != null ? interruptingMove : this.MoveSet.GetAttackForInput(input, this);
                if (_currentAttack != null)
                {
                    if (!_cooldownTimer.Completed && (((int)_currentAttack.Category & _cooldownCategoryMask) != 0))
                    {
                        _currentAttack = null;
                    }
                    else
                    {
                        _cooldownTimer.complete();
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

        // Update Move hitboxes
        if (this.AttackController != null)
            this.AttackController.UpdateHitBoxes(_currentAttack, this.HurtboxState, _facing);

        // Send update finished event (so visual state handling can know to update)
        _updateFinishEvent.CurrentAttack = _currentAttack;
        this.localNotifier.SendEvent(_updateFinishEvent);
    }

    public bool IsGrounded
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.HaltMovementMask) != null; }
    }

    /**
     * Private
     */
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
    private int _cooldownCategoryMask;
    private Timer _autoMoveTimer;
    private Vector2 _autoMoveValue;
    private int _ledgeGrabY;

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

    private void onCollide(LocalEventNotifier.Event e)
    {
        _autoMoveTimer.complete();
    }

    private bool checkAgainstWall(Facing direction)
    {
        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + 1 + this.Hurtbox.Offset.Y - this.Hurtbox.Bounds.Size.Y / 2 - this.AgainstWallCheckOffset);
        bool retVal = this.CollisionManager.CollidePointFirst(checkPoint, this.HaltMovementMask) != null;

        //Debug
        Debug.DrawLine(new Vector3(this.transform.position.x + ((int)direction) * this.Hurtbox.Offset.X, checkPoint.Y, -5), new Vector3(checkPoint.X, checkPoint.Y, -5), retVal ? Color.blue : Color.red, 0.1f);
        return retVal;
    }

    private bool checkAgainstWallForWallJump(Facing direction)
    {
        if (this.WallJumpCheckOffset == this.AgainstWallCheckOffset)
            return true;

        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + 1 + this.Hurtbox.Offset.Y - this.Hurtbox.Bounds.Size.Y / 2 - this.WallJumpCheckOffset);
        return this.CollisionManager.CollidePointFirst(checkPoint, this.HaltMovementMask) != null;
    }

    private bool checkTopHalfLedgeGrab(Facing direction, bool prevGrabbingLedge)
    {
        IntegerVector checkPoint = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + this.Hurtbox.Offset.Y + this.Hurtbox.Bounds.Size.Y / 2 - this.LedgeGrabCheckDistance);
        return this.CollisionManager.CollidePointFirst(checkPoint, this.HaltMovementMask) == null && checkLedgeGrab(direction, prevGrabbingLedge);
    }

    private bool checkLedgeGrab(Facing direction, bool prevGrabbingLedge)
    {
        if (prevGrabbingLedge)
            return true;
        IntegerVector rayOrigin = new Vector2(this.transform.position.x + ((int)direction) * (this.Hurtbox.Offset.X + this.Hurtbox.Bounds.Size.X / 2 + 1), this.transform.position.y + this.Hurtbox.Offset.Y + this.Hurtbox.Bounds.Size.Y / 2 - LedgeGrabCheckDistance);
        CollisionManager.RaycastResult result = this.CollisionManager.RaycastFirst(rayOrigin, Vector2.down, this.Hurtbox.Bounds.Size.Y - this.LedgeGrabCheckDistance + 2, this.HaltMovementMask);
        IntegerVector ledgeTop = result.FarthestPointReached;
        _ledgeGrabY = ledgeTop.Y - this.Hurtbox.Offset.Y - this.Hurtbox.Size.Y / 2 + this.LedgeGrabPeekDistance;
        bool retVal = this.Hurtbox.CollideFirst(0, _ledgeGrabY - Mathf.RoundToInt(this.transform.position.y), this.HaltMovementMask) == null;

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
        _currentAttack = null;
    }

    private void jump()
    {
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

    private void wallJump(Facing direction)
    {
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
        _currentAttack = null;
        _attackTimer.complete();
        _hitStunGravityMultiplier = stunEvent.GravityMultiplier;
        _hitStunAirFrictionMultiplier = stunEvent.AirFrictionMultiplier;
        _hitStunTimer.reset(stunEvent.NumFrames);
    }

    private bool attemptHurtboxStateChange(SCAttack.HurtboxState newState)
    {
        if (newState == this.HurtboxState)
            return true;
        updateHurtboxForState(newState);
        if (this.Hurtbox.CollideFirst(0, 0, this.HaltMovementMask) != null)
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
        switch (state)
        {
            default:
            case SCAttack.HurtboxState.Normal:
                this.Hurtbox.Offset = this.MoveSet.NormalHitboxSpecs.Center;
                this.Hurtbox.Size = this.MoveSet.NormalHitboxSpecs.Size;
                break;
            case SCAttack.HurtboxState.Ducking:
                this.Hurtbox.Offset = this.MoveSet.DuckHitboxSpecs.Center;
                this.Hurtbox.Size = this.MoveSet.DuckHitboxSpecs.Size;
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
                break;
            case SCAttack.VelocityBoost.BoostType.Additive:
                _velocity += boost;
                break;
            case SCAttack.VelocityBoost.BoostType.Average:
                _velocity = (_velocity + boost) / 2.0f;
                break;
            case SCAttack.VelocityBoost.BoostType.Absolute:
                _velocity = boost;
                if (boost.x != 0.0f)
                    return false;
                break;
        }
        return true;
    }
}
