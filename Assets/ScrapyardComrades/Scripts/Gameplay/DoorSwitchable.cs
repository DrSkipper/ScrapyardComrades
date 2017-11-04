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
        _targetSwitchState = Switch.SwitchState.ON;
        _currentSwitchState = Switch.SwitchState.ON;
    }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
        _stateTransitionCooldown.complete();
    }

    void OnReturnToPool()
    {
        close();
        _targetSwitchState = Switch.SwitchState.ON;
        _currentSwitchState = Switch.SwitchState.ON;
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        if (_stateTransitionCooldown.Completed && _currentSwitchState != _targetSwitchState)
        {
            attemptAlignToSwitchState();
        }
    }

    /**
     * Private
     */
    private bool _open;
    private Timer _stateTransitionCooldown;
    private const int STATE_TRANSITION_COOLDOWN = 20;
    private Switch.SwitchState _targetSwitchState;
    private Switch.SwitchState _currentSwitchState;

    private void onStateChange(Switch.SwitchState state)
    {
        _targetSwitchState = state;
    }

    private void attemptAlignToSwitchState()
    {
        switch (_targetSwitchState)
        {
            default:
            case Switch.SwitchState.OFF:
                open();
                _currentSwitchState = _targetSwitchState;
                break;
            case Switch.SwitchState.ON:
                if (tryToClose())
                    _currentSwitchState = _targetSwitchState;
                break;
        }
    }

    private void open()
    {
        _open = true;
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
        _open = false;
        _stateTransitionCooldown.reset();
        _stateTransitionCooldown.start();
        this.integerCollider.enabled = true;
        this.Animator.PlayAnimation(this.CloseAnimation);
    }
}
