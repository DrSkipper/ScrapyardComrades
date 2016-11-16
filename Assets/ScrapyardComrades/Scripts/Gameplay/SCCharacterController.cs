using UnityEngine;
using System;

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
    }

    public float Gravity = 100.0f;
    public float MaxFallSpeed = 500.0f;
    public float FastFallSpeed = 750.0f;
    public float JumpPower = 320.0f;
    public float JumpHorizontalBoost = 50.0f;
    public float JumpHeldGravityMultiplier = 0.5f;
    public float JumpHoldAllowance = 1.0f;
    public int JumpBufferFrames = 6;
    public int JumpGraceFrames = 6;
    public float LandingHorizontalMultiplier = 0.6f;
    public float Friction = 0.2f;
    public float AirFriction = 0.14f;
    public float MaxRunSpeed = 150f;
    public float RunAcceleration = 15.0f;
    public float RunDecceleration = 3.0f;
    public float AirRunAcceleration = 0.1f;

    public AttackController AttackController;
    public IntegerRectCollider Hurtbox;
    public SCMoveSet MoveSet;
    public Facing CurrentFacing { get { return _facing; } }
    public bool OnGround { get { return _onGround; } }
    public int MoveAxis { get { return _moveAxis; } }
    public SCAttack.HurtboxState HurtboxState = SCAttack.HurtboxState.Normal;
    public bool Ducking { get { return _currentAttack == null && this.HurtboxState == SCAttack.HurtboxState.Ducking; } }
    public bool HitStunned { get { return _freezeFrameTimer.Completed && !_hitStunTimer.Completed; } }
    public InputWrapper MostRecentInput { get; private set; }

    public virtual InputWrapper GatherInput()
    {
        return new EmptyInput();
    }

    void Awake()
    {
        _jumpBufferTimer = new Timer(this.JumpBufferFrames);
        _jumpBufferTimer.complete();

        _jumpGraceTimer = new Timer(this.JumpGraceFrames);
        _jumpGraceTimer.complete();

        _attackTimer = new Timer(1);
        _attackTimer.complete();

        _freezeFrameTimer = new Timer(1);
        _freezeFrameTimer.complete();
        _hitStunTimer = new Timer(1);
        _hitStunTimer.complete();

        _updateFinishEvent = new CharacterUpdateFinishedEvent(null);

        this.localNotifier.Listen(FreezeFrameEvent.NAME, this, freezeFrame);
        this.localNotifier.Listen(HitStunEvent.NAME, this, hitStun);

        if (this.AttackController != null)
            this.AttackController.HurtboxChangeCallback = attemptHurtboxStateChange;
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

        _hitStunTimer.update();
        InputWrapper input = !_hitStunTimer.Completed ? new EmptyInput() : this.GatherInput();
        this.MostRecentInput = input;
        _moveAxis = input.MovementAxis;
        _velocity = this.Velocity;
        _onGround = this.IsGrounded;
        bool allowFaceChange = true;

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

        // Friction (No friction if continue to hold in direction of movement in air)
        if (_moveAxis != Math.Sign(_velocity.x) || (_onGround && _moveAxis == 0))
        {
            float maxSlowDown = _onGround ? _parameters.Friction : _parameters.AirFriction;
            _velocity.x = _velocity.x.Approach(0.0f, maxSlowDown);
        }

        // Normal movement
        if (_moveAxis != 0)
        {
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
        if (!_onGround)
        {
            // If jump button is held down use smaller number for gravity
            float gravity = (input.JumpHeld && _canJumpHold && (Math.Sign(_velocity.y) == 1 || Mathf.Abs(_velocity.y) < _parameters.JumpHoldAllowance)) ? (_parameters.JumpHeldGravityMultiplier * _parameters.Gravity) : _parameters.Gravity;
            float targetFallSpeed = _parameters.MaxFallSpeed;

            // Check if we need to fast fall
            if (input.Duck && Math.Sign(_velocity.y) == -1)
                targetFallSpeed = _parameters.FastFallSpeed;

            _velocity.y = _velocity.y.Approach(-targetFallSpeed, -gravity);
        }

        if (_currentAttack == null)
        {
            // Attempt to stand up if necessary
            attemptHurtboxStateChange(SCAttack.HurtboxState.Normal);

            // Check if it's time to jump
            if (input.JumpBegin || !_jumpBufferTimer.Completed)
            {
                if (!_jumpGraceTimer.Completed)
                    jump();
            }

            // Or if we're beginning a Move
            else
            {
                _currentAttack = this.MoveSet.GetAttackForInput(input);
                if (_currentAttack != null)
                {
                    _attackTimer.reset(_currentAttack.NormalFrameLength);
                    _attackTimer.start();

                    // Change direction facing if necessary (need to do this before checking first frame velocity boost)
                    if (_moveAxis != 0)
                        _facing = (Facing)_moveAxis;

                    // Apply velocity boost for current frame in current Move
                    handleVelocityBoosts();
                    allowFaceChange = false;
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
                _currentAttack = null;
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

    private void jump()
    {
        _jumpBufferTimer.complete();
        _jumpGraceTimer.complete();

        _velocity.y = this.JumpPower;

        if (_moveAxis != 0)
            _velocity.x += this.JumpHorizontalBoost * _moveAxis;

        _canJumpHold = true;
    }

    private void freezeFrame(LocalEventNotifier.Event e)
    {
        _freezeFrameTimer.reset((e as FreezeFrameEvent).NumFrames);
    }

    private void hitStun(LocalEventNotifier.Event e)
    {
        HitStunEvent stunEvent = e as HitStunEvent;
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
