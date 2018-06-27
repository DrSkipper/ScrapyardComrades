using UnityEngine;

public class StaticMovingPlatform : VoBehavior, IMovingPlatform
{
    public Vector2 StaticVelociy;
    public Vector2 Velocity { get { return this.StaticVelociy; } }
    public SoundData.Key RidingSfxKey;
    public int SfxInterval = 30;

    void OnSpawn()
    {
        _sfxCycle = 0;
        this.integerCollider.AddToCollisionPool();
    }
    
    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        if (_sfxCycle < this.SfxInterval)
            ++_sfxCycle;
    }

    public void PlayRidingEffects()
    {
        if (_sfxCycle >= this.SfxInterval)
        {
            _sfxCycle = 0;
            SoundManager.Play(this.RidingSfxKey, this.transform);
        }
    }

    /**
     * Private
     */
    private int _sfxCycle;
}
