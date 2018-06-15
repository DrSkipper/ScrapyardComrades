using UnityEngine;

public class SimpleMovingPlatform : VoBehavior, IMovingPlatform
{
    public Vector2 Velocity { get; set; }
    public SoundData.Key RidingSfxKey;
    public int SfxInterval = 30;

    void OnSpawn()
    {
        _sfxCycle = 0;
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
