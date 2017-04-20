using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : VoBehavior
{
    public const string ON_SPAWN_METHOD = "OnSpawn";

    public TimedCallbacks TimedCallbacks;
    public EntityTracker EntityTracker;
    public PooledObject SpriteObjectPrefab;
    public float SpawnDelay = 0.5f;
    public int TileRenderSize = 20;
    //public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(List<NewMapInfo.MapObject> mapObjects, Dictionary<string, PooledObject> prefabs, string quadName, bool trackEntities, string sortingLayerName)
    {
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
                    int x = mapObject.x;
                    int y = mapObject.y;
                    Vector3 spawnPos = new Vector3(x + this.transform.position.x, y + this.transform.position.y, mapObject.z);
                    addSpawn(toSpawn, spawnPos, entity, sortingLayerName, spriteObject ? mapObject.prefab_name : null);
                }
            }
        }
    }

    public void PlaceLights(List<NewMapInfo.MapLight> lights, PooledObject lightPrefab)
    {
        if (lights == null)
            return;

        for (int i = 0; i < lights.Count; ++i)
        {
            NewMapInfo.MapLight light = lights[i];
            addLightSpawn(lightPrefab, new Vector3(light.x + this.transform.position.x, light.y + this.transform.position.y, LIGHTING_Z), light);
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
        _sortingLayerNames.Clear();
    }

    /**
     * Private
     */
    private List<PooledObject> _spawnQueue = new List<PooledObject>();
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private List<EntityTracker.Entity> _spawnEntities = new List<EntityTracker.Entity>();
    private List<string> _spriteNames = new List<string>();
    private List<string> _sortingLayerNames = new List<string>();
    private List<PooledObject> _nonTrackedObjects = new List<PooledObject>();
    private const int LIGHTING_Z = -4;

    private void addSpawn(PooledObject toSpawn, Vector3 spawnPos, EntityTracker.Entity entity, string sortingLayerName, string spriteName = null)
    {
        _spawnQueue.Add(toSpawn);
        _spawnPositions.Add(spawnPos);
        _spawnEntities.Add(entity);
        _spriteNames.Add(spriteName);
        _sortingLayerNames.Add(sortingLayerName);
        this.TimedCallbacks.AddCallback(this, spawn, this.SpawnDelay);
    }

    private void addLightSpawn(PooledObject lightPrefab, Vector3 spawnPos, NewMapInfo.MapLight light)
    {
        PooledObject spawn = lightPrefab.Retain();
        spawn.GetComponent<SCLight>().ConfigureLight(light);
        _nonTrackedObjects.Add(spawn);
        spawn.transform.position = spawnPos;
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
            {
                r.sprite = IndexedSpriteManager.GetSprite(MapEditorManager.PROPS_PATH, spriteName, spriteName);
                r.sortingLayerName = _sortingLayerNames.Pop();
            }
        }
        else
        {
            Renderer r = spawn.GetComponent<Renderer>();
            if (r != null)
                r.sortingLayerName = _sortingLayerNames.Pop();
        }

        spawn.transform.position = spawnLocation;
        spawn.BroadcastMessage(ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    private const string SLASH = "/";
}
