using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation LeftAnimation;
    public SCSpriteAnimation RightAnimation;
    public StaticMovingPlatform MovingPlatform;

    public SwitchListener SwitchListener;
    public SCCharacterController.Facing DefaultDirection;
    public SwitchBehavior OnSwitchAction;

    [System.Serializable]
    public enum SwitchBehavior
    {
        EnableDisable,
        DirectionToggle
    }

    void Awake()
    {
        _velocity = this.MovingPlatform.Velocity;
        _currentFacing = this.DefaultDirection;
        this.SwitchListener.StateChangeCallback += onSwitchStateChange;
    }

    void OnSpawn()
    {
        if (this.OnSwitchAction == SwitchBehavior.EnableDisable)
            _currentFacing = this.DefaultDirection;
    }

    /**
     * Private
     */
    private SCCharacterController.Facing _currentFacing;
    private bool _enabled;
    private Vector2 _velocity;

    private void onSwitchStateChange(Switch.SwitchState state)
    {
        switch (this.OnSwitchAction)
        {
            default:
            case SwitchBehavior.EnableDisable:
                if (state == Switch.SwitchState.OFF)
                    stop();
                else
                    playCurrent();
                break;
            case SwitchBehavior.DirectionToggle:
                _currentFacing = state == Switch.SwitchState.OFF ? SCCharacterController.Facing.Left : SCCharacterController.Facing.Right;
                playCurrent();
                break;
        }
    }

    private void stop()
    {
        this.MovingPlatform.StaticVelociy = Vector2.zero;
        if (!this.Animator.IsPlaying)
            playCurrent();
        this.Animator.Stop();
    }

    private void playCurrent()
    {
        float mult = _currentFacing == SCCharacterController.Facing.Left ? -1 : 1;
        this.MovingPlatform.StaticVelociy = new Vector2(mult * _velocity.x, _velocity.y);
        this.Animator.PlayAnimation(_currentFacing == SCCharacterController.Facing.Left ? this.LeftAnimation : this.RightAnimation);
    }
}
