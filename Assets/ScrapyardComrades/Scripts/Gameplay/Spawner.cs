using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TimedCallbacks))]
public class Spawner : VoBehavior, IPausable
{
    public PooledObject ObjectToSpawn;
    public Transform SpawnLocation;
    public TimedCallbacks TimedCallbacks;
    public float Delay = 0.5f;
    public bool SpawnOnStart = true;
    public bool DestroyAfterSpawn = true;

    void Start()
    {
        if (this.SpawnOnStart)
            this.BeginSpawn();
    }

    public void BeginSpawn()
    {
        this.TimedCallbacks.AddCallback(this, spawn, this.Delay);
    }

    public void ClearSpawnData()
    {
        if (_spawnData != null)
            _spawnData.Clear();
    }

    public void AddSpawnData(string key, string value)
    {
        if (_spawnData == null)
            _spawnData = new Dictionary<string, string>();
        _spawnData.Add(key, value);
    }

    /**
     * Private
     */
    private Dictionary<string, string> _spawnData;

    private void spawn()
    {
        PooledObject spawn = this.ObjectToSpawn.Retain();
        IntegerCollider collider = spawn.GetComponent<IntegerCollider>();
        int yOffset = collider != null ? collider.Bounds.Size.Y / 2 : 0;

        ISpawnable[] spawnables = spawn.GetComponents<ISpawnable>();

        for (int i = 0; i < spawnables.Length; ++i)
        {
            spawnables[i].OnSpawn(_spawnData);
        }

        spawn.transform.position = new Vector3(this.SpawnLocation.position.x, this.SpawnLocation.position.y + yOffset, this.SpawnLocation.position.z);
        if (this.DestroyAfterSpawn)
            ObjectPools.Release(this.gameObject);
    }
}

public interface ISpawnable
{
    void OnSpawn(Dictionary<string, string> spawnData = null);
}
