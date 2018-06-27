using UnityEngine;

public class HeartSpawner : VoBehavior
{
    public PooledObject HeartPrefab;
    public SCConsumable HeartValues;
    public SoundData.Key SpawnSfxKey;

    // Bonus: Additional mutate points our heart gets when dropped, gained by consuming others' hearts. Note that this isn't recorded in SaveData so isn't persistent through saves, but as it's only relevant for enemies eating each others' hearts that should be fine
    public int Bonus { get; set; }

    void Awake()
    {
        this.localNotifier.Listen(SCCharacterController.LOOT_DROP_EVENT, this, onLootDrop);
    }

    /**
     * Private
     */
    private const string HEART_SUFFIX = "_HEART";

    private void onLootDrop(LocalEventNotifier.Event e)
    {
        PooledObject spawn = this.HeartPrefab.Retain();
        Consumable consumable = spawn.GetComponent<Consumable>();
        consumable.Data = this.HeartValues;
        consumable.Bonus = this.Bonus;

        //TODO: Would be cool to load hearts when entering rooms they were left in, even when travelling 2 rooms away or loading from save.
        WorldEntity entity = consumable.GetComponent<WorldEntity>();
        if (EntityTracker.Instance != null)
        {
            WorldEntity ourEntity = this.GetComponent<WorldEntity>();
            if (ourEntity != null)
            {
                string entityName = ourEntity.EntityName + HEART_SUFFIX;
                string quadName = ourEntity.QuadName;
                
                if (EntityTracker.Instance.GetEntity(quadName, entityName, this.HeartPrefab.name).CanLoad)
                {
                    entity.EntityName = entityName;
                    entity.QuadName = quadName;

                    entity.enabled = true;
                    EntityTracker.Instance.TrackLoadedEntity(entity);
                }
                else
                {
                    Debug.LogWarning("Heart getting dropped a second time!");
                    entity.enabled = false;
                }
            }
            else
            {
                entity.enabled = false;
            }
        }
        else
        {
            entity.enabled = false;
        }

        SoundManager.Play(this.SpawnSfxKey, spawn.transform);
        spawn.transform.position = this.transform.position;
        spawn.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }
}
