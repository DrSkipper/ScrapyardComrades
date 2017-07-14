using UnityEngine;

public class InventoryController : MonoBehaviour, IKey
{
    public int InventorySize;
    public PooledObject ItemPrefab;

    public int NumItems { get { return _currentItemCount; } }
    public SCPickup GetItem(int index) { return _inventory[index]; }

    void Awake()
    {
        _inventory = new SCPickup[this.InventorySize];
    }

    public void PickupItem(SCPickup pickup)
    {
        bool foundEmpty = false;
        for (int i = 0; i < _inventory.Length; ++i)
        {
            if (_inventory[i] == null)
            {
                foundEmpty = true;
                _inventory[i] = pickup;
                ++_currentItemCount;
                break;
            }
        }
        if (!foundEmpty)
            _inventory[0] = pickup;
    }

    public PooledObject UseItem(int index)
    {
        SCPickup item = null;
        if (_inventory[index] != null)
        {
            item = _inventory[index];
            _inventory[index] = null;
            --_currentItemCount;
        }

        if (item != null)
        {
            PooledObject itemObject = this.ItemPrefab.Retain();
            itemObject.GetComponent<Pickup>().Data = item;
            return itemObject;
        }

        return null;
    }

    void OnReturnToPool()
    {
        for (int i = 0; i < _inventory.Length; ++i)
            _inventory[i] = null;
        _currentItemCount = 0;
    }

    public bool CanOpen(SCPickup.KeyType lockType)
    {
        if (lockType != SCPickup.KeyType.None)
        {
            for (int i = 0; i < _inventory.Length; ++i)
            {
                if (_inventory[i] != null && _inventory[i].Key == lockType)
                    return true;
            }
        }
        return false;
    }

    /**
     * Private
     */
    private int _currentItemCount;
    private SCPickup[] _inventory;
}
