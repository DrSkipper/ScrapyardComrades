using UnityEngine;

public class Explosion : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;

    void OnSpawn()
    {
        _hasRun = false;
    }

    void FixedUpdate()
    {
        if (!_hasRun)
        {
            _hasRun = true;

            GameObject collided = this.integerCollider.CollideFirst(0, 0, this.DamagableLayers);
            if (collided != null)
            {
                Damagable d = collided.GetComponent<Damagable>();
                if (d != null)
                {
                    d.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, (SCCharacterController.Facing)Mathf.RoundToInt(Mathf.Sign(this.transform.position.x - collided.transform.position.x)));
                }
            }
        }
    }

    /**
     * Private
     */
    private bool _hasRun;
}
