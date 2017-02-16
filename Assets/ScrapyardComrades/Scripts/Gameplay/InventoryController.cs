﻿using UnityEngine;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    public int InventorySize;
    public Pickup[] ItemPrefabs;

    public int NumItems { get { return _currentItemCount; } }
    public SCPickup GetItem(int index) { return _inventory[index]; }

    void Awake()
    {
        _inventory = new SCPickup[this.InventorySize];
        _itemPrefabs = new Dictionary<string, Pickup>();
        for (int i = 0; i < this.ItemPrefabs.Length; ++i)
        {
            Pickup prefab = this.ItemPrefabs[i];
            _itemPrefabs.Add(prefab.Data.Name, prefab);
        }
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
            Pickup prefab = _itemPrefabs[item.Name];
            return prefab.GetComponent<PooledObject>().Retain();
        }

        return null;
    }

    void OnReturnToPool()
    {
        for (int i = 0; i < _inventory.Length; ++i)
            _inventory[i] = null;
        _currentItemCount = 0;
    }

    /**
     * Private
     */
    private int _currentItemCount;
    private SCPickup[] _inventory;
    private Dictionary<string, Pickup> _itemPrefabs;
}
