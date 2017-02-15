using UnityEngine;
using UnityEngine.UI;

public class InventoryVisualizer : MonoBehaviour
{
    public Image[] Slots;
    public Image[] SlotContents;

    void Update()
    {
        GameObject player = PlayerReference.GameObject;

        if (_inventory == null)
        {
            if (player != null)
                _inventory = player.GetComponent<InventoryController>();
        }
        else if (player == null)
        {
            _inventory = null;
        }

        updateState();
    }

    /**
     * Private
     */
    private InventoryController _inventory;

    private void updateState()
    {
        for (int i = 0; i < this.SlotContents.Length; ++i)
        {
            if (_inventory != null && i < _inventory.NumItems)
            {
                this.Slots[i].enabled = true;
                this.SlotContents[i].enabled = true; //TODO: Change image to sprite for item
            }
            else if (_inventory != null && i < _inventory.InventorySize)
            {
                this.Slots[i].enabled = true;
                this.SlotContents[i].enabled = false;
            }
            else
            {
                this.Slots[i].enabled = false;
                this.SlotContents[i].enabled = false;
            }
        }
    }
}
