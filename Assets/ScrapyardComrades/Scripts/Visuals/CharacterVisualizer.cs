using UnityEngine;

[RequireComponent(typeof(SCSpriteAnimator))]
[RequireComponent(typeof(SCCharacterController))]
public class CharacterVisualizer : VoBehavior
{
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation RunAnimation;
    public SCSpriteAnimation JumpAnimation;
    public SCSpriteAnimation FallAnimation;
    public SCSpriteAnimation DuckAnimation;
    public SCSpriteAnimation HitStunAnimation;

    private const string IDLE_STATE = "idle";
    private const string RUNNING_STATE = "run";
    private const string JUMPING_STATE = "jump";
    private const string FALLING_STATE = "fall";
    private const string DUCKING_STATE = "duck";
    private const string STUNNED_STATE = "stun";
    private const string ATTACK_STATE = "attack";

    void Awake()
    {
        _characterController = this.GetComponent<SCCharacterController>();
        _spriteAnimator = this.GetComponent<SCSpriteAnimator>();
        _stateMachine = new FSMStateMachine();
        _stateMachine.AddState(IDLE_STATE, this.updateGeneric, this.enterIdle);
        _stateMachine.AddState(RUNNING_STATE, this.updateGeneric, this.enterRunning);
        _stateMachine.AddState(JUMPING_STATE, this.updateGeneric, this.enterJumping);
        _stateMachine.AddState(FALLING_STATE, this.updateGeneric, this.enterFalling);
        _stateMachine.AddState(DUCKING_STATE, this.updateGeneric, this.enterDucking);
        _stateMachine.AddState(STUNNED_STATE, this.updateGeneric, this.enterHitStun);
        _stateMachine.AddState(ATTACK_STATE, this.updateAttack, this.enterAttack);
        _stateMachine.BeginWithInitialState(IDLE_STATE);

        this.localNotifier.Listen(CharacterUpdateFinishedEvent.NAME, this, this.UpdateVisual);
    }

    public void UpdateVisual(LocalEventNotifier.Event localEvent)
    {
        SCAttack attack = ((CharacterUpdateFinishedEvent)localEvent).CurrentAttack;
        _attackChanged = _currentAttack != null && _currentAttack != attack;
        _currentAttack = attack;

        _stateMachine.Update();
        this.transform.localScale = new Vector3((_characterController.CurrentFacing == SCCharacterController.Facing.Left ? -1 : 1) * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
    }

    /**
     * Private
     */
    private SCCharacterController _characterController;
    private FSMStateMachine _stateMachine;
    private SCSpriteAnimator _spriteAnimator;
    private SCAttack _currentAttack;
    private bool _attackChanged;

    private string updateGeneric()
    {
        if (_currentAttack != null)
        {
            return ATTACK_STATE;
        }
        if (_characterController.HitStunned)
        {
            return STUNNED_STATE;
        }
        if (_characterController.Ducking)
        {
            return DUCKING_STATE;
        }
        if (!_characterController.OnGround)
        {
            if (GameplayInput.JumpHeld && _characterController.Velocity.y > 0.0f)
                return JUMPING_STATE;
            return FALLING_STATE;
        }
        {
            return RUNNING_STATE;
        }
        return IDLE_STATE;
    }

    private string updateAttack()
    {
        if (_currentAttack == null)
            return updateGeneric();

        if (_attackChanged)
            _spriteAnimator.PlayAnimation(_currentAttack.SpriteAnimation);

        return ATTACK_STATE;
    }

    private void enterAttack()
    {
        _spriteAnimator.PlayAnimation(_currentAttack.SpriteAnimation);
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

    private void enterDucking()
    {
        _spriteAnimator.PlayAnimation(this.DuckAnimation);
    }

    private void enterHitStun()
    {
        _spriteAnimator.PlayAnimation(this.HitStunAnimation);
    }
}
