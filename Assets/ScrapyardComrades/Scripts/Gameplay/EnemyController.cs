using System.Collections.Generic;

public class EnemyController : SCCharacterController
{
    public const string QUAD_NAME_KEY = "quad";
    public const string ENEMY_NAME_KEY = "name";

    public override void OnSpawn(Dictionary<string, string> spawnData = null)
    {
        base.OnSpawn(spawnData);
        _deathEvent.QuadName = spawnData[QUAD_NAME_KEY];
        _deathEvent.EnemyName = spawnData[ENEMY_NAME_KEY];
    }

    public override void OnDeath()
    {
        base.OnDeath();
        GlobalEvents.Notifier.SendEvent(_deathEvent);
    }

    /**
     * Private
     */
    private EnemyDiedEvent _deathEvent = new EnemyDiedEvent("", "");
}
