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
    public SoundData.Key PowerOnSfxKey;
    public SoundData.Key PowerOffSfxKey;

    [System.Serializable]
    public enum SwitchBehavior
    {
        EnableDisable,
        DirectionToggle
    }

    void Awake()
    {
        _currentFacing = this.DefaultDirection;
        this.SwitchListener.StateChangeCallback += onSwitchStateChange;
    }

    void OnSpawn()
    {
        _v = this.MovingPlatform.StaticVelociy;
        if (this.OnSwitchAction == SwitchBehavior.EnableDisable)
            _currentFacing = this.DefaultDirection;
    }

    void OnReturnToPool()
    {
        _playingSfx = false;
        this.MovingPlatform.StaticVelociy = _v;
    }

    void FixedUpdate()
    {
        if (!_playingSfx)
            _playingSfx = true;
    }

    /**
     * Private
     */
    private SCCharacterController.Facing _currentFacing;
    private Vector2 _v;
    private bool _playingSfx;

    private void onSwitchStateChange(Switch.SwitchState state)
    {
        switch (this.OnSwitchAction)
        {
            default:
            case SwitchBehavior.EnableDisable:
                if (state == Switch.SwitchState.OFF)
                {
                    if (_playingSfx)
                        SoundManager.Play(this.PowerOffSfxKey, this.transform);
                    stop();
                }
                else
                {
                    if (_playingSfx)
                        SoundManager.Play(this.PowerOnSfxKey, this.transform);
                    playCurrent();
                }
                break;
            case SwitchBehavior.DirectionToggle:
                _currentFacing = state == Switch.SwitchState.OFF ? SCCharacterController.Facing.Left : SCCharacterController.Facing.Right;
                playCurrent();
                break;
        }
    }

    private void stop()
    {
        if (!this.Animator.IsPlaying)
            playCurrent();
        this.MovingPlatform.StaticVelociy = Vector2.zero;
        this.Animator.Stop();
    }

    private void playCurrent()
    {
        float mult = _currentFacing == SCCharacterController.Facing.Left ? -1 : 1;
        this.MovingPlatform.StaticVelociy = new Vector2(mult * Mathf.Abs(_v.x), mult * Mathf.Abs(_v.y));
        this.Animator.PlayAnimation(_currentFacing == SCCharacterController.Facing.Left ? this.LeftAnimation : this.RightAnimation);
    }
}
