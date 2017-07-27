using UnityEngine;
using System.Collections.Generic;

public class EntityTracker : MonoBehaviour
{
    public const string PLAYER = "player";
    public List<string> PlayerObjectNames;
    public static EntityTracker Instance { get; private set; }
    public bool IsInitialLoad { get; private set; }
    
    public class Entity
    {
        public SaveData.EntityModel EntityData;
        public bool AttemptingLoad;
        public bool Loaded;
        public bool CanLoad { get { return !this.EntityData.Consumed && !this.Loaded && !this.AttemptingLoad; } }

        public Entity(SaveData.EntityModel entityData)
        {
            this.EntityData = entityData;
            this.Loaded = false;
            this.AttemptingLoad = false;
        }
    }

    void Awake()
    {
        Instance = this;
        if (!SaveData.DataLoaded)
            SaveData.LoadFromDisk(SaveData.DEBUG_SLOT_NAME);
        GlobalEvents.Notifier.Listen(EntityConsumedEvent.NAME, this, entityConsumed);
        GlobalEvents.Notifier.Listen(EntityReplacementEvent.NAME, this, entityReplaced);
    }

    public Entity GetEntity(string quadName, string entityName, string prefabName)
    {
        // Players owned globally, not by quad
        if (this.PlayerObjectNames.Contains(prefabName))
        {
            quadName = PLAYER;
            entityName = PLAYER;
        }

        Dictionary<string, Entity> quadEntities;
        if (!_trackedEntities.ContainsKey(quadName))
        {
            quadEntities = new Dictionary<string, Entity>();
            _trackedEntities.Add(quadName, quadEntities);
        }
        else
        {
            quadEntities = _trackedEntities[quadName];
        }

        Entity entity;
        if (!quadEntities.ContainsKey(entityName))
        {
            SaveData.EntityModel entityData = SaveData.GetTrackedEntity(quadName, entityName);
            entity = new Entity(entityData);
            quadEntities.Add(entityName, entity);
        }
        else
        {
            entity = quadEntities[entityName];
        }

        return entity;
    }

    public void TrackLoadedEntity(WorldEntity entity)
    {
        _loadedEntities.Add(entity);
        _trackedEntities[entity.QuadName][entity.EntityName].Loaded = true;
    }

    public void QuadsUnloaded(List<WorldLoadingManager.MapQuad> currentLoadedQuads, WorldLoadingManager.MapQuad centerQuad, int tileRenderSize)
    {
        for (int i = 0; i < _loadedEntities.Count;)
        {
            WorldEntity entity = _loadedEntities[i];
            IntegerVector entityPos = ((Vector2)entity.transform.position) / tileRenderSize;
            bool stillLoaded = false;

            for (int q = 0; q < currentLoadedQuads.Count; ++q)
            {
                IntegerRect bounds = currentLoadedQuads[q].GetRelativeBounds(centerQuad);
                if (bounds.Contains(entityPos))
                {
                    stillLoaded = true;
                    break;
                }
            }

            if (!stillLoaded)
            {
                _trackedEntities[entity.QuadName][entity.EntityName].Loaded = false;
                ObjectPools.Release(entity.gameObject);
                _loadedEntities.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void DisableOutOfBounds(WorldLoadingManager.MapQuad centerQuad, int tileRenderSize, WorldLoadingManager.MapQuad prevQuad = null)
    {
        for (int i = 0; i < _loadedEntities.Count; ++i)
        {
            WorldEntity entity = _loadedEntities[i];
            IntegerVector entityPos = ((Vector2)entity.transform.position) / tileRenderSize;
            bool inBounds = centerQuad.CenteredBounds.Contains(entityPos, 2);
            if (!inBounds && prevQuad != null)
                inBounds = prevQuad.GetRelativeBounds(centerQuad).Contains(entityPos, 2);
            entity.gameObject.SetActive(inBounds);
        }
    }

    public string GetEntityStateTag(string quadName, string entityName)
    {
        return _trackedEntities[quadName][entityName].EntityData.StateTag;
    }

    public void SetEntityStateTag(string quadName, string entityName, string stateTag)
    {
        _trackedEntities[quadName][entityName].EntityData.StateTag = stateTag;
    }

    public void BeginInitialLoad()
    {
        this.IsInitialLoad = true;
    }

    public void EndInitialLoad()
    {
        this.IsInitialLoad = false;
    }

    /**
     * Private
     */
    private Dictionary<string, Dictionary<string, Entity>> _trackedEntities = new Dictionary<string, Dictionary<string, Entity>>();
    private List<WorldEntity> _loadedEntities = new List<WorldEntity>();

    private void entityReplaced(LocalEventNotifier.Event e)
    {
        WorldEntity newEntity = (e as EntityReplacementEvent).NewEntity;
        for (int i = 0; i < _loadedEntities.Count; ++i)
        {
            if (_loadedEntities[i].QuadName == newEntity.QuadName && _loadedEntities[i].EntityName == newEntity.EntityName)
            {
                _loadedEntities[i] = newEntity;
                break;
            }
        }
    }

    private void entityConsumed(LocalEventNotifier.Event e)
    {
        EntityConsumedEvent consumedEvent = e as EntityConsumedEvent;

        if (!_trackedEntities.ContainsKey(consumedEvent.QuadName))
        {
            Debug.LogWarning("EntityTracker notified of consumption with invalid quad owner name: " + consumedEvent.QuadName + ". Entity name is: " + consumedEvent.EntityName);
        }
        else if (!_trackedEntities[consumedEvent.QuadName].ContainsKey(consumedEvent.EntityName))
        {
            Debug.LogWarning("EntityTracker notified of consumption of invalid entity named: " + consumedEvent.EntityName + ", owned by quad: " + consumedEvent.QuadName);
        }
        else
        {
            _trackedEntities[consumedEvent.QuadName][consumedEvent.EntityName].EntityData.Consumed = true;
            for (int i = 0; i < _loadedEntities.Count; ++i)
            {
                if (_loadedEntities[i].QuadName == consumedEvent.QuadName && _loadedEntities[i].EntityName == consumedEvent.EntityName)
                {
                    _loadedEntities.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
