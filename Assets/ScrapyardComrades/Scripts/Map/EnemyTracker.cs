using UnityEngine;
using System.Collections.Generic;

public class EnemyTracker : MonoBehaviour
{
    void Awake()
    {
        GlobalEvents.Notifier.Listen(EnemyDiedEvent.NAME, this, enemyDied);
    }

    public bool AttemptLoad(string quadName, string enemyName)
    {
        if (!_trackedEntries.ContainsKey(quadName))
        {
            _trackedEntries.Add(quadName, new Dictionary<string, EnemyEntry>());
        }

        if (!_trackedEntries[quadName].ContainsKey(enemyName))
        {
            _trackedEntries[quadName].Add(enemyName, new EnemyEntry(enemyName));
        }

        return _trackedEntries[quadName][enemyName].IsAlive;
    }

    /**
     * Private
     */
    private Dictionary<string, Dictionary<string, EnemyEntry>> _trackedEntries = new Dictionary<string, Dictionary<string, EnemyEntry>>();

    private class EnemyEntry
    {
        public string Name;
        public bool IsAlive;

        public EnemyEntry(string name)
        {
            this.Name = name;
            this.IsAlive = true;
        }
    }

    private void enemyDied(LocalEventNotifier.Event e)
    {
        EnemyDiedEvent deathEvent = e as EnemyDiedEvent;

        if (!_trackedEntries.ContainsKey(deathEvent.QuadName))
        {
            Debug.LogWarning("Enemy tracker notified of enemy death in invalid quad name: " + deathEvent.QuadName + ". Enemy name is: " + deathEvent.EnemyName);
        }
        else if (!_trackedEntries[deathEvent.QuadName].ContainsKey(deathEvent.EnemyName))
        {
            Debug.LogWarning("Enemy tracker notified of death of invalid enemy named: " + deathEvent.EnemyName + ", in quad: " + deathEvent.QuadName);
        }
        else
        {
            _trackedEntries[deathEvent.QuadName][deathEvent.EnemyName].IsAlive = false;
        }
    }
}
