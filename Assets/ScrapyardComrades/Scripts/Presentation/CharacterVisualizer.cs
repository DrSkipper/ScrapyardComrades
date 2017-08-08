using UnityEngine;

[RequireComponent(typeof(SCSpriteAnimator))]
[RequireComponent(typeof(SCCharacterController))]
public class CharacterVisualizer : VoBehavior
{
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation RunAnimation;
    public SCSpriteAnimation JumpAnimation;
    public SCSpriteAnimation FallAnimation;
    public SCSpriteAnimation WallSlideAnimation;
    public SCSpriteAnimation LedgeGrabAnimation;
    public SCSpriteAnimation LedgeGrabBackAnimation;
    public SCSpriteAnimation DuckAnimation;
    public SCSpriteAnimation HitStunAnimation;
    public SCSpriteAnimation DeathHitStunAnimation;
    public SCSpriteAnimation DeathAnimation;
    public SCSpriteAnimation StandupAnimation;
    public float DodgeAlpha = 0.8f;

    private const string IDLE_STATE = "idle";
    private const string RUNNING_STATE = "run";
    private const string JUMPING_STATE = "jump";
    private const string FALLING_STATE = "fall";
    private const string WALLSLIDE_STATE = "wallslide";
    private const string LEDGEGRAB_STATE = "ledgegrab";
    private const string LEDGEGRABBACK_STATE = "ledgegrabback";
    private const string DUCKING_STATE = "duck";
    private const string STUNNED_STATE = "stun";
    private const string DEATHSTUN_STATE = "deathstun";
    private const string DEATH_STATE = "death";
    private const string ATTACK_STATE = "attack";
    private const string STANDUP_STATE = "stand";

    void Awake()
    {
        _characterController = this.GetComponent<SCCharacterController>();
        _spriteAnimator = this.GetComponent<SCSpriteAnimator>();
        _stateMachine = new FSMStateMachine();
        _stateMachine.AddState(IDLE_STATE, this.updateGeneric, this.enterIdle);
        _stateMachine.AddState(RUNNING_STATE, this.updateGeneric, this.enterRunning);
        _stateMachine.AddState(JUMPING_STATE, this.updateGeneric, this.enterJumping);
        _stateMachine.AddState(FALLING_STATE, this.updateGeneric, this.enterFalling);
        _stateMachine.AddState(WALLSLIDE_STATE, this.updateGeneric, this.enterWallSlide);
        _stateMachine.AddState(LEDGEGRAB_STATE, this.updateGeneric, this.enterLedgeGrab);
        _stateMachine.AddState(LEDGEGRABBACK_STATE, this.updateGeneric, this.enterLedgeGrabBack, this.exitLedgeGrabBack);
        _stateMachine.AddState(DUCKING_STATE, this.updateGeneric, this.enterDucking);
        _stateMachine.AddState(STUNNED_STATE, this.updateGeneric, this.enterHitStun);
        _stateMachine.AddState(DEATHSTUN_STATE, this.updateGeneric, this.enterDeathStun);
        _stateMachine.AddState(DEATH_STATE, this.updateDying, this.enterDeath);
        _stateMachine.AddState(ATTACK_STATE, this.updateAttack, this.enterAttack, this.exitAttack);
        _stateMachine.AddState(STANDUP_STATE, this.updateStandup, this.enterStandup);
        _stateMachine.BeginWithInitialState(IDLE_STATE);

        this.localNotifier.Listen(CharacterUpdateFinishedEvent.NAME, this, this.UpdateVisual);
    }

    void OnSpawn()
    {
        _stateMachine.CurrentState = _characterController.StandupOnSpawn ? STANDUP_STATE : IDLE_STATE;
    }

    void OnReturnToPool()
    {
        _stateMachine.CurrentState = IDLE_STATE;
    }

    public void UpdateVisual(LocalEventNotifier.Event localEvent)
    {
        SCAttack attack = ((CharacterUpdateFinishedEvent)localEvent).CurrentAttack;
        _attackChanged = _currentAttack != null && _currentAttack != attack;
        _currentAttack = attack;

        _stateMachine.Update();
        this.transform.localScale = new Vector3(_facingModifier * (_characterController.CurrentFacing == SCCharacterController.Facing.Left ? -1 : 1) * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
    }

    /**
     * Private
     */
    private SCCharacterController _characterController;
    private FSMStateMachine _stateMachine;
    private SCSpriteAnimator _spriteAnimator;
    private SCAttack _currentAttack;
    private bool _attackChanged;
    private int _facingModifier = 1;

    private string updateGeneric()
    {
        if (_currentAttack != null)
        {
            return ATTACK_STATE;
        }
        if (_characterController.HitStunned)
        {
            if (_characterController.Damagable.Dead)
            {
                if (_characterController.OnGround && Mathf.Abs(_characterController.Velocity.x) < SCCharacterController.DEATH_VELOCITY_MAX)
                    return DEATH_STATE;
                return DEATHSTUN_STATE;
            }
            return STUNNED_STATE;
        }
        if (_characterController.Ducking)
        {
            return DUCKING_STATE;
        }
        if (!_characterController.OnGround)
        {
            if (_characterController.IsGrabbingLedge)
                return _characterController.DirectionGrabbingLedge == _characterController.CurrentFacing ? LEDGEGRAB_STATE : LEDGEGRABBACK_STATE;
            if (_characterController.Velocity.y > 0.0f)
                return JUMPING_STATE;
            if (_characterController.IsWallSliding)
                return WALLSLIDE_STATE;
            return FALLING_STATE;
        }
        if (_characterController.MoveAxis != 0)
        {
            return RUNNING_STATE;
        }
        return IDLE_STATE;
    }

    private string updateDying()
    {
        return _characterController.OnGround ? DEATH_STATE : DEATHSTUN_STATE;
    }

    private string updateAttack()
    {
        if (_currentAttack == null)
            return updateGeneric();

        if (_attackChanged)
        {
            Color c = this.spriteRenderer.color;
            c.a = _currentAttack.Category == SCAttack.MoveCategory.Dodge ? this.DodgeAlpha : 1.0f;
            this.spriteRenderer.color = c;
            _spriteAnimator.PlayAnimation(_currentAttack.SpriteAnimation);
        }

        return ATTACK_STATE;
    }

    private string updateStandup()
    {
        return _characterController.HitStunned ? STANDUP_STATE : IDLE_STATE;
    }

    private void enterStandup()
    {
        _spriteAnimator.PlayAnimation(this.StandupAnimation);
    }

    private void enterAttack()
    {
        if (_currentAttack.Category == SCAttack.MoveCategory.Dodge)
        {
            Color c = this.spriteRenderer.color;
            c.a = this.DodgeAlpha;
            this.spriteRenderer.color = c;
        }
        _spriteAnimator.PlayAnimation(_currentAttack.SpriteAnimation);
    }

    private void exitAttack()
    {
        Color c = this.spriteRenderer.color;
        c.a = 1.0f;
        this.spriteRenderer.color = c;
    }

    private void enterIdle()
    {
        //TODO - Handle transitions from Previous State
        _spriteAnimator.PlayAnimation(this.IdleAnimation);
    }

    private void enterRunning()
    {
        _spriteAnimator.PlayAnimation(this.RunAnimation);
    }

    private void enterJumping()
    {
        _spriteAnimator.PlayAnimation(this.JumpAnimation);
    }

    private void enterFalling()
    {
        _spriteAnimator.PlayAnimation(this.FallAnimation);
    }

    private void enterWallSlide()
    {
        _spriteAnimator.PlayAnimation(this.WallSlideAnimation);
    }

    private void enterLedgeGrab()
    {
        _spriteAnimator.PlayAnimation(this.LedgeGrabAnimation);
    }

    private void enterLedgeGrabBack()
    {
        _facingModifier = -1;
        _spriteAnimator.PlayAnimation(this.LedgeGrabBackAnimation != null ? this.LedgeGrabBackAnimation : this.LedgeGrabAnimation);
    }

    private void exitLedgeGrabBack()
    {
        _facingModifier = 1;
    }

    private void enterDucking()
    {
        _spriteAnimator.PlayAnimation(this.DuckAnimation);
    }

    private void enterHitStun()
    {
        _spriteAnimator.PlayAnimation(this.HitStunAnimation);
    }

    private void enterDeathStun()
    {
        _spriteAnimator.PlayAnimation(this.DeathHitStunAnimation);
    }

    private void enterDeath()
    {
        _spriteAnimator.PlayAnimation(this.DeathAnimation);
    }
}
