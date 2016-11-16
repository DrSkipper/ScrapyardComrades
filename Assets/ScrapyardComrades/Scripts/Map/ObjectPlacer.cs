using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : VoBehavior
{
    public PooledObject PlayerPrefab;
    public PooledObject EnemyPrefab;
    public PooledObject HeartPrefab;
    public TimedCallbacks TimedCallbacks;
    public EntityTracker EntityTracker;
    public float SpawnDelay = 0.5f;
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(MapInfo.MapObject[] mapObjects, int gridWidth, int gridHeight, string quadName)
    {
        int positionCorrection = this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Length; ++i)
        {
            MapInfo.MapObject mapObject = mapObjects[i];
            EntityTracker.Entity entity = this.EntityTracker.GetEntity(quadName, mapObject.name);

            if (entity.CanLoad)
            {
                PooledObject toSpawn = null;

                if (mapObject.name == EntityTracker.PLAYER)
                    toSpawn = this.PlayerPrefab;
                else if (mapObject.name.Contains(EntityTracker.ENEMY))
                    toSpawn = this.EnemyPrefab;
                else if (mapObject.name.Contains(EntityTracker.HEART))
                    toSpawn = this.HeartPrefab;

                if (toSpawn != null)
                {
                    entity.AttemptingLoad = true;
                    int x = (mapObject.x + mapObject.width / 2) * positionCorrection;
                    int y = !this.FlipVertical ?
                        (mapObject.y + mapObject.height) * positionCorrection :
                        gridHeight * this.TileRenderSize - ((mapObject.y) * positionCorrection) + (1 * positionCorrection);
                    Vector3 spawnPos = new Vector3(x + this.transform.position.x, y + this.transform.position.y);
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
        EntityTracker.Entity entity = _spawnEntities.Pop();
        entity.AttemptingLoad = false;

        PooledObject spawn = toSpawn.Retain();
        WorldEntity worldEntity = spawn.GetComponent<WorldEntity>();
        worldEntity.QuadName = entity.QuadName;
        worldEntity.EntityName = entity.EntityName;
        this.EntityTracker.TrackLoadedEntity(worldEntity);
        IntegerCollider collider = spawn.GetComponent<IntegerCollider>();
        int yOffset = collider != null ? collider.Bounds.Size.Y / 2 : 0;

        ISpawnable[] spawnables = spawn.GetComponents<ISpawnable>();

        for (int i = 0; i < spawnables.Length; ++i)
        {
            spawnables[i].OnSpawn();
        }

        spawn.transform.position = new Vector3(spawnLocation.x, spawnLocation.y + yOffset, spawnLocation.z);
    }
}

public interface ISpawnable
{
    void OnSpawn();
}
