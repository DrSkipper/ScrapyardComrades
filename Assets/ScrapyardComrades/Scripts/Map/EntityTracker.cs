using UnityEngine;
using System.Collections.Generic;

public class EntityTracker : MonoBehaviour
{
    public class Entity
    {
        public string Name;
        public bool Consumed;
        //TODO - Health remaining
        public bool Loaded;
        public bool CanLoad { get { return !this.Consumed && !this.Loaded; } }

        public Entity(string name)
        {
            this.Name = name;
            this.Consumed = false;
            this.Loaded = false;
        }
    }

    void Awake()
    {
        GlobalEvents.Notifier.Listen(EntityConsumedEvent.NAME, this, entityConsumed);
    }

    public bool CanLoadEntity(string quadName, string entityName)
    {
        if (!_trackedEntities.ContainsKey(quadName))
        {
            _trackedEntities.Add(quadName, new Dictionary<string, Entity>());
        }

        if (!_trackedEntities[quadName].ContainsKey(entityName))
        {
            _trackedEntities[quadName].Add(entityName, new Entity(entityName));
        }

        return _trackedEntities[quadName][entityName].CanLoad;
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

    /**
     * Private
     */
    private Dictionary<string, Dictionary<string, Entity>> _trackedEntities = new Dictionary<string, Dictionary<string, Entity>>();
    private List<WorldEntity> _loadedEntities = new List<WorldEntity>();

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
