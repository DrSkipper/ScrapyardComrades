using UnityEngine;
using System.Collections.Generic;

public class Explosion : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public LayerMask BlockingLayers;
    public SCAttack.HitData HitData;

    void OnSpawn()
    {
        if (_collisions == null)
            _collisions = new List<GameObject>();
        _hasRun = false;
    }

    void FixedUpdate()
    {
        if (!_hasRun)
        {
            _hasRun = true;

            this.integerCollider.Collide(_collisions, 0, 0, this.DamagableLayers);
            for (int i = 0; i < _collisions.Count; ++i)
            {
                GameObject collided = _collisions[i];
                Damagable d = collided.GetComponent<Damagable>();
                if (d != null)
                {
                    IntegerVector pos = this.integerPosition;
                    Vector2 diff = d.integerPosition - pos;

                    // Make sure we don't hurt though walls
                    if (!CollisionManager.RaycastFirst(pos, diff.normalized, diff.magnitude, this.BlockingLayers).Collided)
                        d.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, (SCCharacterController.Facing)Mathf.RoundToInt(Mathf.Sign(this.transform.position.x - collided.transform.position.x)));
                }
            }
            _collisions.Clear();
        }
    }

    /**
     * Private
     */
    private bool _hasRun;
    private List<GameObject> _collisions;
}
