using UnityEngine;

public class PowerupObject : MonoBehaviour
{
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation PickupAnimation;
    public IntegerCollider Collider;
    public LayerMask ConsumerMask;
    public int VerticalSpeed = 1;

    void OnSpawn()
    {
        if (_destructionTimer == null)
            _destructionTimer = new Timer(this.PickupAnimation.LengthInFrames, false, false, onDestruction);
        else
            _destructionTimer.Paused = true;
    }

    void FixedUpdate()
    {
        if (_destructionTimer.IsRunning)
        {
            _destructionTimer.update();
            this.transform.SetY(this.transform.position.y + this.VerticalSpeed);
        }
        else
        {
            GameObject collided = this.Collider.CollideFirst(0, 0, this.ConsumerMask);
            if (collided != null)
            {
                //TODO: increment player's powerup count

                this.Animator.PlayAnimation(this.PickupAnimation);
                _destructionTimer.resetAndStart();
            }
        }
    }

    /**
     * Private
     */
    private Timer _destructionTimer;

    private void onDestruction()
    {
        ObjectPools.Release(this.gameObject);
    }
}
