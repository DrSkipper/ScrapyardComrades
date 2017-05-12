using UnityEngine;

public class HeartSpawner : VoBehavior
{
    public PooledObject HeartPrefab;
    public SCConsumable HeartValues;

    void Awake()
    {
        this.localNotifier.Listen(SCCharacterController.LOOT_DROP_EVENT, this, onLootDrop);
    }

    private void onLootDrop(LocalEventNotifier.Event e)
    {
        PooledObject spawn = this.HeartPrefab.Retain();
        Consumable consumable = spawn.GetComponent<Consumable>();
        consumable.Data = this.HeartValues;

        //TODO: Register the heart's world entity using this object's QuadName, and using this object's EntityName as a prefix for the heart's EntityName? Would require system of remembering positions of objects created during gameplay.
        consumable.GetComponent<WorldEntity>().enabled = false;

        spawn.transform.position = this.transform.position;
        spawn.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }
}
