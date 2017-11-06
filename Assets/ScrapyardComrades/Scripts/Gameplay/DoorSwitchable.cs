using UnityEngine;

public class DoorSwitchable : VoBehavior
{
    public LayerMask BlockDoorMask;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation OpenAnimation;
    public SCSpriteAnimation CloseAnimation;

    private void Awake()
    {
        this.GetComponent<SwitchListener>().StateChangeCallback += onStateChange;
        _stateTransitionCooldown = new Timer(STATE_TRANSITION_COOLDOWN, false, false);
    }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
        _stateTransitionCooldown.complete();
    }

    void OnReturnToPool()
    {
        close();
        this.integerCollider.RemoveFromCollisionPool();
    }

    /**
     * Private
     */
    private Timer _stateTransitionCooldown;
    private const int STATE_TRANSITION_COOLDOWN = 20;
    private Switch.SwitchState _targetSwitchState;
    private Switch.SwitchState _currentSwitchState;

    private void onStateChange(Switch.SwitchState state)
    {
        _targetSwitchState = state;
        attemptAlignToSwitchState();
    }

    private void attemptAlignToSwitchState()
    {
        switch (_targetSwitchState)
        {
            default:
            case Switch.SwitchState.OFF:
                open();
                break;
            case Switch.SwitchState.ON:
                tryToClose();
                break;
        }
    }

    private void open()
    {
        _currentSwitchState = Switch.SwitchState.OFF;
        _stateTransitionCooldown.reset();
        _stateTransitionCooldown.start();
        this.integerCollider.enabled = false;
        this.Animator.PlayAnimation(this.OpenAnimation);
    }

    private bool tryToClose()
    {
        GameObject collided = this.integerCollider.CollideFirst(0, 0, this.BlockDoorMask);

        if (collided == null)
        {
            close();
            return true;
        }
        return false;
    }

    private void close()
    {
        _currentSwitchState = Switch.SwitchState.ON;
        _stateTransitionCooldown.reset();
        _stateTransitionCooldown.start();
        this.integerCollider.enabled = true;
        this.Animator.PlayAnimation(this.CloseAnimation);
    }
}
