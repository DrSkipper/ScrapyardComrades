using UnityEngine;

public class Mine : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;
    public PooledObject ExplosionEffect;
    public Transform ExplosionLocation;

    void FixedUpdate()
    {
        GameObject collided = this.integerCollider.CollideFirst(0, 0, this.DamagableLayers);
        if (collided != null)
        {
            Damagable damagable = collided.GetComponent<Damagable>();
            if (damagable != null)
            {
                damagable.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, this.transform.position.x > collided.transform.position.x ? SCCharacterController.Facing.Left : SCCharacterController.Facing.Right);

                explode();
            }
        }
    }

    private void explode()
    {
        PooledObject explosion = this.ExplosionEffect.Retain();
        explosion.transform.position = this.ExplosionLocation.position;
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        ObjectPools.Release(this.gameObject);
    }
}
