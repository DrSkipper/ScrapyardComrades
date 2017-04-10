﻿using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : VoBehavior
{
    public TimedCallbacks TimedCallbacks;
    public EntityTracker EntityTracker;
    public PooledObject SpriteObjectPrefab;
    public float SpawnDelay = 0.5f;
    public int TileRenderSize = 20;
    //public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(List<NewMapInfo.MapObject> mapObjects, Dictionary<string, PooledObject> prefabs, string quadName, bool trackEntities)
    {
        //TODO: Globalize setting of MapEditor tile render size, In-Game tile render size, and tile texture sizes (if tile texture size is ever actually even needed at this point)
        int positionCorrection = 1; // this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Count; ++i)
        {
            NewMapInfo.MapObject mapObject = mapObjects[i];
            EntityTracker.Entity entity = null;
            if (trackEntities)
                entity = this.EntityTracker.GetEntity(quadName, mapObject.name, mapObject.prefab_name);

            if (!trackEntities || entity.CanLoad)
            {
                PooledObject toSpawn;
                bool spriteObject = false;
                if (prefabs.ContainsKey(mapObject.prefab_name))
                {
                    toSpawn = prefabs[mapObject.prefab_name];
                }
                else
                {
                    toSpawn = this.SpriteObjectPrefab;
                    spriteObject = true;
                }

                if (toSpawn != null)
                {
                    if (trackEntities)
                        entity.AttemptingLoad = true;
                    int x = mapObject.x * positionCorrection;
                    int y = mapObject.y * positionCorrection;
                    Vector3 spawnPos = new Vector3(x + this.transform.position.x, y + this.transform.position.y, mapObject.z);
                    addSpawn(toSpawn, spawnPos, entity, spriteObject ? mapObject.prefab_name : null);
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
        for (int i = 0; i < _nonTrackedObjects.Count; ++i)
        {
            _nonTrackedObjects[i].Release();
        }
        _nonTrackedObjects.Clear();
        _spawnEntities.Clear();
        _spawnQueue.Clear();
        _spawnPositions.Clear();
        _spriteNames.Clear();
    }

    /**
     * Private
     */
    private List<PooledObject> _spawnQueue = new List<PooledObject>();
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private List<EntityTracker.Entity> _spawnEntities = new List<EntityTracker.Entity>();
    private List<string> _spriteNames = new List<string>();
    private List<PooledObject> _nonTrackedObjects = new List<PooledObject>();

    private void addSpawn(PooledObject toSpawn, Vector3 spawnPos, EntityTracker.Entity entity, string spriteName = null)
    {
        _spawnQueue.Add(toSpawn);
        _spawnPositions.Add(spawnPos);
        _spawnEntities.Add(entity);
        _spriteNames.Add(spriteName);
        this.TimedCallbacks.AddCallback(this, spawn, this.SpawnDelay);
    }

    private void spawn()
    {
        PooledObject toSpawn = _spawnQueue.Pop();
        Vector3 spawnLocation = _spawnPositions.Pop();
        PooledObject spawn = toSpawn.Retain();
        EntityTracker.Entity entity = _spawnEntities.Pop();
        string spriteName = _spriteNames.Pop();

        if (entity != null)
        {
            entity.AttemptingLoad = false;
            WorldEntity worldEntity = spawn.GetComponent<WorldEntity>();
            worldEntity.QuadName = entity.QuadName;
            worldEntity.EntityName = entity.EntityName;
            this.EntityTracker.TrackLoadedEntity(worldEntity);
        }
        else
        {
            _nonTrackedObjects.Add(spawn);
        }

        if (spriteName != null)
        {
            SpriteRenderer r = spawn.GetComponent<SpriteRenderer>();
            if (r != null)
                r.sprite = Resources.Load<Sprite>(MapEditorManager.PROPS_FOLDER + SLASH + spriteName);
        }

        ISpawnable[] spawnables = spawn.GetComponents<ISpawnable>();

        for (int i = 0; i < spawnables.Length; ++i)
        {
            spawnables[i].OnSpawn();
        }

        spawn.transform.position = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z);
    }

    private const string SLASH = "/";
}

public interface ISpawnable
{
    void OnSpawn();
}
