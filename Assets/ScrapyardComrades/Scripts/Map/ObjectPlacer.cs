using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : VoBehavior
{
    //TODO: Convert to dictionary at some point
    public PooledObject[] ObjectPrefabs;
    public TimedCallbacks TimedCallbacks;
    public EntityTracker EntityTracker;
    public float SpawnDelay = 0.5f;
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(List<NewMapInfo.MapObject> mapObjects, string quadName, bool trackEntities)
    {
        int positionCorrection = this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Count; ++i)
        {
            NewMapInfo.MapObject mapObject = mapObjects[i];
            EntityTracker.Entity entity = null;
            if (trackEntities)
                entity = this.EntityTracker.GetEntity(quadName, mapObject.name, mapObject.prefab_name);

            if (!trackEntities || entity.CanLoad)
            {
                PooledObject toSpawn = null;
                for (int j = 0; j < this.ObjectPrefabs.Length; ++j)
                {
                    if (this.ObjectPrefabs[j].name == mapObject.prefab_name)
                    {
                        toSpawn = this.ObjectPrefabs[j];
                        break;
                    }
                }

                if (toSpawn != null)
                {
                    if (trackEntities)
                        entity.AttemptingLoad = true;
                    int x = mapObject.x * positionCorrection;
                    int y = mapObject.y * positionCorrection;
                    Vector3 spawnPos = new Vector3(x + this.transform.position.x, y + this.transform.position.y, mapObject.z);
                    addSpawn(toSpawn, spawnPos, entity);
                }
            }
        }
    }

    public void WipeSpawns()
    {
        this.TimedCallbacks.RemoveCallbacksForOwner(this);
        for (int i = 0; i < _spawnEntities.Count; ++i)
        {
            _spawnEntities[i].AttemptingLoad = false;
        }
        _spawnEntities.Clear();
        _spawnQueue.Clear();
        _spawnPositions.Clear();
    }

    /**
     * Private
     */
    private List<PooledObject> _spawnQueue = new List<PooledObject>();
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private List<EntityTracker.Entity> _spawnEntities = new List<EntityTracker.Entity>();

    private void addSpawn(PooledObject toSpawn, Vector3 spawnPos, EntityTracker.Entity entity)
    {
        _spawnQueue.Insert(0, toSpawn);
        _spawnPositions.Insert(0, spawnPos);
        _spawnEntities.Insert(0, entity);
        this.TimedCallbacks.AddCallback(this, spawn, this.SpawnDelay);
    }

    private void spawn()
    {
        PooledObject toSpawn = _spawnQueue.Pop();
        Vector3 spawnLocation = _spawnPositions.Pop();
        PooledObject spawn = toSpawn.Retain();
        EntityTracker.Entity entity = _spawnEntities.Pop();
        if (entity != null)
        {
            entity.AttemptingLoad = false;
            WorldEntity worldEntity = spawn.GetComponent<WorldEntity>();
            worldEntity.QuadName = entity.QuadName;
            worldEntity.EntityName = entity.EntityName;
            this.EntityTracker.TrackLoadedEntity(worldEntity);
        }

        ISpawnable[] spawnables = spawn.GetComponents<ISpawnable>();

        for (int i = 0; i < spawnables.Length; ++i)
        {
            spawnables[i].OnSpawn();
        }

        spawn.transform.position = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z);
    }
}

public interface ISpawnable
{
    void OnSpawn();
}
