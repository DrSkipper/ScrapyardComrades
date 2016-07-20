using UnityEngine;
using System;
using Rewired;

public class PlayerController : Actor2D
{
    public enum Facing
    {
        Left = -1,
        Right = 1
    }

    public struct VelocityBoost
    {
        int EffectFrame;
        Vector2 Boost;
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

    public SCMoveSet MoveSet;
    public Facing CurrentFacing { get { return _facing; } }
    public bool OnGround { get { return _onGround; } }
    public float MoveAxis { get { return _moveAxis; } }

    void Awake()
    {
        _jumpBufferTimer = new Timer(this.JumpBufferFrames);
        _jumpBufferTimer.complete();

        _jumpGraceTimer = new Timer(this.JumpGraceFrames);
        _jumpGraceTimer.complete();

        _attackTimer = new Timer(1);
        _attackTimer.complete();
    }

    public override void Update()
    {
        _moveAxis = GameplayInput.MovementAxis;
        _velocity = this.Velocity;
        _onGround = this.IsGrounded;

        updateControlParametersForCurrentAttack();
        _velocity += _parameters.VelocityBoost;

        // Update jumpBufferCounter, and if input indicates Jump is pressed, set it to JUMP_BUFFER (6)
        _jumpBufferTimer.update(1/*SCPhysics.DeltaFrames*/);
        if (GameplayInput.JumpBegin)
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
            _jumpGraceTimer.update(1/*SCPhysics.DeltaFrames*/);
        }

        // Friction (No friction if continue to hold in direction of movement in air)
        if (_moveAxis != Math.Sign(_velocity.x) || (_onGround && _moveAxis == 0))
        {
            float maxSlowDown = _onGround ? _parameters.Friction : _parameters.AirFriction;
            _velocity.x = _velocity.x.Approach(0.0f, maxSlowDown * 1/*SCPhysics.DeltaFrames*/);
        }
        
        // Normal movement
        if (_moveAxis != 0)
        {
            // Deccel if past max speed
            if (Mathf.Abs(_velocity.x) > _parameters.MaxRunSpeed && Math.Sign(_velocity.x) == _moveAxis)
            {
                _velocity.x = _velocity.x.Approach(_parameters.MaxRunSpeed * _moveAxis, _parameters.RunDecceleration * 1/*SCPhysics.DeltaFrames*/);
            }

            // Accelerate
            else
            {
                float acceleration = _onGround ? _parameters.RunAcceleration : _parameters.AirRunAcceleration;
                _velocity.x = _velocity.x.Approach(_parameters.MaxRunSpeed * _moveAxis, acceleration *= 1/*SCPhysics.DeltaFrames*/);
            }
        }

        // Handle falling
        if (!_onGround)
        {
            // If jump button is held down use smaller number for gravity
            float gravity = (GameplayInput.JumpHeld && _canJumpHold && (Math.Sign(_velocity.y) == 1 || Mathf.Abs(_velocity.y) < _parameters.JumpHoldAllowance)) ? (_parameters.JumpHeldGravityMultiplier * _parameters.Gravity) : _parameters.Gravity;
            float targetFallSpeed = _parameters.MaxFallSpeed;
            
            // Check if we need to fast fall
            if (GameplayInput.Duck && Math.Sign(_velocity.y) == -1)
                targetFallSpeed = _parameters.FastFallSpeed;

            _velocity.y = _velocity.y.Approach(-targetFallSpeed, -gravity * 1/*SCPhysics.DeltaFrames*/);
        }

        if (_currentAttack == null)
        {
            // Check if it's time to jump
            if (GameplayInput.JumpBegin || !_jumpBufferTimer.Completed)
            {
                if (!_jumpGraceTimer.Completed)
                    jump();
            }

            // Or if we're beginning an attack
            else if (GameplayInput.AttackLightBegin)
            {
                _currentAttack = this.MoveSet.GroundNeutral;
                _attackTimer.reset(_currentAttack.NormalFrameLength);
                _attackTimer.start();
            }
        }
        else
        {
            // Update the attack
            _attackTimer.update(1/*SCPhysics.DeltaFrames*/);

            if (_attackTimer.Completed)
                _currentAttack = null;
        }

        if (_moveAxis != 0)
            _facing = (Facing)_moveAxis;

        this.Velocity = _velocity;
        base.Update();

        this.localNotifier.SendEvent(new PlayerUpdateFinishedEvent(_currentAttack));
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
    private Timer _attackTimer;
    private ControlParameters _parameters;

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
    }

    private void updateControlParametersForCurrentAttack()
    {
        if (_currentAttack != null)
        {
            _parameters.Gravity = this.Gravity * _currentAttack.GravityMultiplier;
            _parameters.JumpPower = this.JumpPower * _currentAttack.JumpPowerMultiplier;
            _parameters.JumpHorizontalBoost = this.JumpHorizontalBoost * _currentAttack.JumpHorizontalBoostMultiplier;
            _parameters.Friction = this.Friction * _currentAttack.FrictionMultiplier;
            _parameters.AirFriction = this.AirFriction * _currentAttack.AirFrictionMultiplier;
            _parameters.MaxRunSpeed = this.MaxRunSpeed * _currentAttack.MaxRunSpeedMultiplier;
            _parameters.RunAcceleration = this.RunAcceleration * _currentAttack.RunAccelerationMultiplier;
            _parameters.RunDecceleration = this.RunDecceleration * _currentAttack.RunDeccelerationMultiplier;
            _parameters.AirRunAcceleration = this.AirRunAcceleration * _currentAttack.AirRunAccelerationMultiplier;

            _parameters.VelocityBoost = Vector2.zero;
            //if (_currentAttack.VelocityBoosts != null && _currentAttack)
        }
        else
        {
            _parameters.Gravity = this.Gravity;
            _parameters.JumpPower = this.JumpPower;
            _parameters.JumpHorizontalBoost = this.JumpHorizontalBoost;
            _parameters.Friction = this.Friction;
            _parameters.AirFriction = this.AirFriction;
            _parameters.MaxRunSpeed = this.MaxRunSpeed;
            _parameters.RunAcceleration = this.RunAcceleration;
            _parameters.RunDecceleration = this.RunDecceleration;
            _parameters.AirRunAcceleration = this.AirRunAcceleration;
            _parameters.VelocityBoost = Vector2.zero;
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
}
