using UnityEngine;

public class HitFlashSpawner : VoBehavior
{
    public PooledObject FlashPrefab;
    public Color[] PossibleColors;
    public int LocationRange = 2;
    public int HitSpawnCount = 1;
    public int DeathSpawnCount = 3;
    public int HitFlashInterval = 0;
    public int DeathFlashInterval = 10;

    void Awake()
    {
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHit);
    }

    void onHit(LocalEventNotifier.Event e)
    {
        HitStunEvent hitEvent = e as HitStunEvent;
        int numToSpawn = hitEvent.Dead ? this.DeathSpawnCount : this.HitSpawnCount;
        int interval = hitEvent.Dead ? this.DeathFlashInterval : this.HitFlashInterval;

        while (numToSpawn > 0)
        {
            --numToSpawn;
            int x = Random.Range(hitEvent.HitPos.X - this.LocationRange, hitEvent.HitPos.X + this.LocationRange + 1);
            int y = Random.Range(hitEvent.HitPos.Y - this.LocationRange, hitEvent.HitPos.Y + this.LocationRange + 1);
            Color color = this.PossibleColors[Random.Range(0, this.PossibleColors.Length)];
            spawn(interval * numToSpawn, new IntegerVector(x, y), color);
        }
    }

    /**
     * Private
     */
    private void spawn(int delay, IntegerVector pos, Color color)
    {
        PooledObject flash = this.FlashPrefab.Retain();
        flash.transform.SetPosition2D(pos);
        flash.GetComponent<LightFlash>().StartWithDelayAndColor(delay, color);
        flash.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }
}
