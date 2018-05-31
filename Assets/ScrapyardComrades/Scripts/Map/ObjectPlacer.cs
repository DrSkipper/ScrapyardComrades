using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : VoBehavior
{
    public const string ON_SPAWN_METHOD = "OnSpawn";

    public TimedCallbacks TimedCallbacks;
    public EntityTracker EntityTracker;
    public PooledObject SpriteObjectPrefab;
    public PooledObject AnimationObjectPrefab;
    public float SpawnDelay = 0.5f;
    public int TileRenderSize = 20;
    //public int TileTextureSize = 10;
    public bool FlipVertical = true;
    public SpriteAnimationCollection Animations;

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
                else if (IndexedSpriteManager.HasAtlas(MapEditorManager.PROPS_PATH, mapObject.prefab_name))
                {
                    toSpawn = this.SpriteObjectPrefab;
                    spriteObject = true;
                }
                else
                {
                    toSpawn = this.AnimationObjectPrefab;
                    spriteObject = true;
                }

                if (toSpawn != null)
                {
                    if (trackEntities)
                        entity.AttemptingLoad = true;
                    int x = mapObject.x;
                    int y = mapObject.y;
                    Vector3 spawnPos = new Vector3(x + this.transform.position.x, y + this.transform.position.y, mapObject.z);
                    IntegerVector secondaryPos = new IntegerVector(mapObject.secondary_x + Mathf.RoundToInt(this.transform.position.x), mapObject.secondary_y + Mathf.RoundToInt(this.transform.position.y));
                    addSpawn(toSpawn, spawnPos, entity, sortingLayerName, mapObject.parameters, spriteObject ? mapObject.prefab_name : null, secondaryPos);
                }
            }
        }

        this.TimedCallbacks.AddCallback(this, spawnAll, this.SpawnDelay);
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
            if (_spawnEntities[i] != null)
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
        _spawnParams.Clear();
        _secondaryPositions.Clear();
    }

    public void EnableNonTrackedObjects(bool enabled)
    {
        for (int i = 0; i < _nonTrackedObjects.Count; ++i)
        {
            if (_nonTrackedObjects[i].GetComponent<SCParallaxObject>() == null)
                _nonTrackedObjects[i].gameObject.SetActive(enabled);
        }
    }

    /**
     * Private
     */
    private List<PooledObject> _spawnQueue = new List<PooledObject>();
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private List<NewMapInfo.ObjectParam[]> _spawnParams = new List<NewMapInfo.ObjectParam[]>();
    private List<EntityTracker.Entity> _spawnEntities = new List<EntityTracker.Entity>();
    private List<string> _spriteNames = new List<string>();
    private List<string> _sortingLayerNames = new List<string>();
    private List<PooledObject> _nonTrackedObjects = new List<PooledObject>();
    private List<IntegerVector> _secondaryPositions = new List<IntegerVector>();
    private const int LIGHTING_Z = -4;
    private static int SORTING_IN_LAYER = MAX_SORTING_ORDER;
    private const int MIN_SORTING_ORDER = -10000;
    private const int MAX_SORTING_ORDER = 10000;

    private void addSpawn(PooledObject toSpawn, Vector3 spawnPos, EntityTracker.Entity entity, string sortingLayerName, NewMapInfo.ObjectParam[] spawnParams, string spriteName, IntegerVector secondaryPosition)
    {
        _spawnQueue.Add(toSpawn);
        _spawnPositions.Add(spawnPos);
        _spawnEntities.Add(entity);
        _spriteNames.Add(spriteName);
        _sortingLayerNames.Add(sortingLayerName);
        _spawnParams.Add(spawnParams);
        _secondaryPositions.Add(secondaryPosition);
    }

    private void addLightSpawn(PooledObject lightPrefab, Vector3 spawnPos, NewMapInfo.MapLight light)
    {
        PooledObject spawn = lightPrefab.Retain();
        spawn.GetComponent<SCLight>().ConfigureLight(light);
        _nonTrackedObjects.Add(spawn);
        spawn.transform.position = spawnPos;
        spawn.BroadcastMessage(ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    private void spawnAll()
    {
        while (_spawnQueue.Count > 0)
            spawn();
    }

    private void spawn()
    {
        PooledObject toSpawn = _spawnQueue.Pop();
        Vector3 spawnLocation = _spawnPositions.Pop();
        PooledObject spawn = toSpawn.Retain();
        EntityTracker.Entity entity = _spawnEntities.Pop();
        string spriteName = _spriteNames.Pop();
        NewMapInfo.ObjectParam[] spawnParams = _spawnParams.Pop();
        IntegerVector secondaryPos = _secondaryPositions.Pop();

        if (entity != null)
        {
            entity.AttemptingLoad = false;
            WorldEntity worldEntity = spawn.GetComponent<WorldEntity>();
            worldEntity.QuadName = entity.EntityData.QuadName;
            worldEntity.EntityName = entity.EntityData.EntityName;
            this.EntityTracker.TrackLoadedEntity(worldEntity);
        }
        else
        {
            _nonTrackedObjects.Add(spawn);
        }

        if (spriteName != null)
        {
            SpriteRenderer r = spawn.GetComponent<SpriteRenderer>();
            if (toSpawn == this.SpriteObjectPrefab)
            {
                // Sprite object
                r.sprite = IndexedSpriteManager.GetSprite(MapEditorManager.PROPS_PATH, spriteName, spriteName);
            }
            else
            {
                // Animation ojbect
                spawn.GetComponent<SCSpriteAnimator>().DefaultAnimation = this.Animations.Animations.Find(x => x.name == spriteName);
            }

            r.sortingLayerName = _sortingLayerNames.Pop();
            r.sortingOrder = --SORTING_IN_LAYER;
        }
        else
        {
            Renderer r = spawn.GetComponent<Renderer>();
            if (r != null)
            {
                r.sortingLayerName = _sortingLayerNames.Pop();

                if (r.sortingOrder == 0)
                    r.sortingOrder = --SORTING_IN_LAYER;
            }
        }

        //spawnLocation.z += spawn.transform.position.z;
        spawn.transform.position = spawnLocation;

        ObjectConfigurer configurer = spawn.GetComponent<ObjectConfigurer>();
        if (configurer != null)
        {
            if (configurer.GetSecondaryTransform() != null)
                configurer.GetSecondaryTransform().SetPosition2D(secondaryPos);
            configurer.ConfigureForParams(spawnParams);
        }

        if (SORTING_IN_LAYER <= MIN_SORTING_ORDER)
            SORTING_IN_LAYER = MAX_SORTING_ORDER;
        
        spawn.BroadcastMessage(ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    private const string SLASH = "/";
}
