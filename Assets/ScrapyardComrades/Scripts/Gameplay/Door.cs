using UnityEngine;
using System.Collections.Generic;

public class Door : VoBehavior, IPausable
{
    public SCPickup.KeyType LockType;
    public IntegerCollider KeyRangeCollider;
    public IntegerCollider DoorCollider;
    public LayerMask KeyMask;
    public LayerMask BlockDoorMask;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation OpenAnimation;
    public SCSpriteAnimation CloseAnimation;

    void Awake()
    {
        _collisions = new List<GameObject>();
        _stateTransitionCooldown = new Timer(STATE_TRANSITION_COOLDOWN, false, false);
    }

    void OnSpawn()
    {
        close();
        _stateTransitionCooldown.complete();
        this.DoorCollider.AddToCollisionPool();
        this.Animator.PlayAnimation(this.OpenAnimation);
        this.Animator.Stop();
    }

    void OnReturnToPool()
    {
        this.DoorCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        if (_stateTransitionCooldown.Completed)
        {
            this.KeyRangeCollider.Collide(_collisions, 0, 0, this.KeyMask);
            if (!_open)
            {
                for (int i = 0; i < _collisions.Count; ++i)
                {
                    IKey key = _collisions[i].GetComponent<IKey>();
                    if (key != null && key.CanOpen(this.LockType))
                    {
                        open();
                        break;
                    }
                }
            }
            else
            {
                bool stayOpen = false;
                for (int i = 0; i < _collisions.Count; ++i)
                {
                    IKey key = _collisions[i].GetComponent<IKey>();
                    if (key != null && key.CanOpen(this.LockType))
                    {
                        stayOpen = true;
                        break;
                    }
                }

                if (!stayOpen)
                    tryToClose();
            }
            _collisions.Clear();
        }
        else
        {
            _stateTransitionCooldown.update();
        }
    }

    /**
     * Private
     */
    private bool _open;
    private Timer _stateTransitionCooldown;
    private List<GameObject> _collisions;
    private const int STATE_TRANSITION_COOLDOWN = 20;

    private void open()
    {
        _open = true;
        _stateTransitionCooldown.reset();
        _stateTransitionCooldown.start();
        this.DoorCollider.enabled = false;
        this.Animator.PlayAnimation(this.OpenAnimation);
    }

    private void tryToClose()
    {
        GameObject collided = this.DoorCollider.CollideFirst(0, 0, this.BlockDoorMask);

        if (collided == null)
            close();
    }

    private void close()
    {
        _open = false;
        _stateTransitionCooldown.reset();
        _stateTransitionCooldown.start();
        this.DoorCollider.enabled = true;
        this.Animator.PlayAnimation(this.CloseAnimation);
    }
}
