using UnityEngine;

public class PlayerController : SCCharacterController, PowerupConsumer
{
    public PowerupLevels PowerupLevels;
    public string GlowParameter;
    public float GlowPowerBase = 0.1f;
    public float GlowPowerPerTier = 0.1f;
    public float GlowPowerLerpSpeed = 0.01f;

    public struct PlayerInput : InputWrapper
    {
        public int MovementAxis { get { return GameplayInput.MovementAxis; } }
        public bool JumpBegin { get { return GameplayInput.JumpBegin; } }
        public bool JumpHeld { get { return GameplayInput.JumpHeld; } }
        public bool DodgeBegin { get { return GameplayInput.DodgeBegin; } }
        public bool DodgeHeld { get { return GameplayInput.DodgeHeld; } }
        public bool Duck { get { return GameplayInput.Duck; } }
        public bool LookUp { get { return GameplayInput.LookUp; } }
        public bool AttackLightBegin { get { return GameplayInput.AttackLightBegin; } }
        public bool AttackLightHeld { get { return GameplayInput.AttackLightHeld; } }
        public bool AttackStrongBegin { get { return GameplayInput.AttackStrongBegin; } }
        public bool AttackStrongHeld { get { return GameplayInput.AttackStrongHeld; } }
        public bool UseItem { get { return GameplayInput.UseItem; } }
        public bool Interact { get { return GameplayInput.Interact; } }
        public bool PausePressed { get { return GameplayInput.PausePressed; } }

        public static PlayerInput Reference = new PlayerInput();
    }

    public override InputWrapper GatherInput()
    {
        return PlayerInput.Reference;
    }

    public override void Awake()
    {
        base.Awake();
        _glowMaterial = this.spriteRenderer.material;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        _glowAmt = 0.0f;
        GlobalEvents.Notifier.SendEvent(new PlayerSpawnedEvent(this.gameObject));
        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onWorldRecenter);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, WorldRecenterEvent.NAME);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!_died && this.Damagable.Dead)
        {
            _died = true;
            GlobalEvents.Notifier.SendEvent(new PlayerDiedEvent());
        }

        if (_glowMaterial != null)
        {
            float target = _powerupTier > 0 ? this.GlowPowerBase + _powerupTier * this.GlowPowerPerTier : 0.0f;
            _glowAmt = _glowAmt.Approach(target, this.GlowPowerLerpSpeed);
            _glowMaterial.SetFloat(this.GlowParameter, _glowAmt);
        }
    }

    public void ConsumePowerup()
    {
        ++SaveData.PlayerStats.PowerupCount;
        int powerupLevel = Mathf.Clamp(SaveData.PlayerStats.PowerupCount, 0, this.PowerupLevels.TiersByPowerupLevel.Length - 1);
        _powerupTier = Mathf.Min(_powerupTier, this.PowerupLevels.TiersByPowerupLevel[powerupLevel].Tiers.Length - 1);
        _framesAtNextPowerupTier = 0;
        _framesBelowDowngrade = 0;
    }

    /**
     * Private
     */
    private bool _died;
    private int _framesAtNextPowerupTier;
    private int _framesBelowDowngrade;
    private int _powerupTier;
    private Material _glowMaterial;
    private float _glowAmt;

    private const float SCREEN_TRANSITION_UP_BOOST = 3.0f;

    private void onWorldRecenter(LocalEventNotifier.Event e)
    {
        if (this.Velocity.y > 0 && (e as WorldRecenterEvent).RecenterOffset.Y < 0)
            this.Velocity.y += SCREEN_TRANSITION_UP_BOOST;
    }

    protected override void updateControlParameters()
    {
        base.updateControlParameters();

        // Apply parameter modifications based on power level
        int powerupLevel = Mathf.Clamp(SaveData.PlayerStats.PowerupCount, 0, this.PowerupLevels.TiersByPowerupLevel.Length - 1);
        PowerupTiers tiers = this.PowerupLevels.TiersByPowerupLevel[powerupLevel];

        updatePowerupState(tiers);
        if (_powerupTier > 0)
            applyCurrentPowerupState(tiers);
    }

    private void updatePowerupState(PowerupTiers tiers)
    {
        bool inNextTier = false;
        float speed = this.TotalVelocity.magnitude;

        // Check if we can advance our powerup tier
        if (_powerupTier + 1 < tiers.Tiers.Length)
        {
            _framesBelowDowngrade = 0;
            PowerupTiers.Tier nextTier = tiers.Tiers[_powerupTier + 1];

            if (nextTier.MinVelocityToTrigger <= speed)
            {
                inNextTier = true;
                ++_framesAtNextPowerupTier;

                // Advance tier if necessary
                if (_framesAtNextPowerupTier > nextTier.MinDurationAtVelocity)
                    changePowerupTier(1);
            }
        }

        // Check if we must downgrade our powerup tier
        if (!inNextTier)
        {
            _framesAtNextPowerupTier = 0;

            if (_powerupTier > 0)
            {
                PowerupTiers.Tier currentTier = tiers.Tiers[_powerupTier];

                if (currentTier.MinDurationAtVelocity > speed)
                {
                    ++_framesBelowDowngrade;

                    // Downgrade tier if necessary
                    if (_framesBelowDowngrade > currentTier.DurationBelowVelocityToDowngrade)
                        changePowerupTier(-_powerupTier);
                }
                else
                {
                    _framesBelowDowngrade = 0;
                }
            }
        }
    }

    private void changePowerupTier(int direction)
    {
        _powerupTier += direction;
        _framesAtNextPowerupTier = 0;
        _framesBelowDowngrade = 0;
    }

    private void applyCurrentPowerupState(PowerupTiers tiers)
    {
        PowerupState powerupState = tiers.Tiers[_powerupTier].State;

        //TODO: Apply Damage Multiplier
        _parameters.Friction *= powerupState.FrictionMultiplier;
        _parameters.AirFriction *= powerupState.AirFrictionMultiplier;
        _parameters.Gravity *= powerupState.GravityMultiplier;
        _parameters.JumpHorizontalBoost *= powerupState.JumpHorizontalBoostMultiplier;
        _parameters.MaxSpeedForJumpHorizontalBoost *= powerupState.MaxSpeedForJumpHorizontalBoostMultiplier;
        _parameters.LandingHorizontalMultiplier *= powerupState.LandingHorizontalMultiplierMultiplier;
        _parameters.JumpPower *= powerupState.JumpPowerMultiplier;
        _parameters.WallJumpYPower *= powerupState.WallJumpYPowerMultiplier;
        _parameters.WallJumpXPower *= powerupState.WallJumpXPowerMultiplier;
        _parameters.MaxFallSpeed *= powerupState.MaxFallSpeedMultiplier;
        _parameters.FastFallSpeed *= powerupState.FastFallSpeedMultiplier;
        _parameters.MaxRunSpeed *= powerupState.MaxRunSpeedMultiplier;
        _parameters.RunAcceleration *= powerupState.RunAccelerationMultiplier;
        _parameters.RunDecceleration *= powerupState.RunDeccelerationMultiplier;
        _parameters.AirRunAcceleration *= powerupState.AirRunAccelerationMultiplier;
    }
}
