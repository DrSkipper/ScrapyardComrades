using UnityEngine;
using System.Collections.Generic;

public class EntityTracker : MonoBehaviour
{
    public const string PLAYER = "player";
    public List<string> PlayerObjectNames;

    public class Entity
    {
        public string QuadName;
        public string EntityName;
        public bool Consumed;
        //TODO - Health remaining, other persistent variables
        public bool AttemptingLoad;
        public bool Loaded;
        public bool CanLoad { get { return !this.Consumed && !this.Loaded && !this.AttemptingLoad; } }

        public Entity(string quadName, string entityName)
        {
            this.QuadName = quadName;
            this.EntityName = entityName;
            this.Consumed = false;
            this.Loaded = false;
            this.AttemptingLoad = false;
        }
    }

    void Awake()
    {
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

        if (!_trackedEntities.ContainsKey(quadName))
        {
            _trackedEntities.Add(quadName, new Dictionary<string, Entity>());
        }

        if (!_trackedEntities[quadName].ContainsKey(entityName))
        {
            _trackedEntities[quadName].Add(entityName, new Entity(quadName, entityName));
        }

        return _trackedEntities[quadName][entityName];
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

    public void DisableOutOfBounds(WorldLoadingManager.MapQuad centerQuad, int tileRenderSize)
    {
        for (int i = 0; i < _loadedEntities.Count; ++i)
        {
            WorldEntity entity = _loadedEntities[i];
            IntegerVector entityPos = ((Vector2)entity.transform.position) / tileRenderSize;
            entity.gameObject.SetActive(centerQuad.CenteredBounds.Contains(entityPos, 2));
        }
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
            _trackedEntities[consumedEvent.QuadName][consumedEvent.EntityName].Consumed = true;
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
