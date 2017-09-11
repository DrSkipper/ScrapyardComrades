using UnityEngine;
using System.Collections.Generic;

public class UISaveSlotMenu : Menu
{
    public const string EMPTY_SLOT = "empty";
    public const string EMPTY_SLOT_NAME = "Start New Game";
    public const string ERASE = "ERASE";
    public MenuMode Mode;
    public string EraseMenu = "EraseMenu";

    [System.Serializable]
    public enum MenuMode
    {
        Loading,
        Erasing
    }

    void Awake()
    {
        this.Reload();
    }

    public override void Reload()
    {
        if (_slots == null)
            _slots = new Dictionary<string, SaveSlotData.SlotSummary>();
        else
            _slots.Clear();
        _slotsArray = SaveSlotData.GetAllSlots();

        for (int i = 0; i < _slotsArray.Length; ++i)
        {
            _slots[_slotsArray[i].Name] = _slotsArray[i];
        }

        this.Elements = new MenuElement[SaveSlotData.MAX_SLOTS + (this.Mode == MenuMode.Loading ? 1 : 0)];
        string prefix = this.Mode == MenuMode.Loading ? StringExtensions.EMPTY : "Erase ";
        string emptyName = this.Mode == MenuMode.Loading ? EMPTY_SLOT_NAME : "Empty Slot";

        for (int i = 0; i < SaveSlotData.MAX_SLOTS; ++i)
        {
            MenuElement element = new MenuElement();
            Action action = new Action();
            action.Type = ActionType.Custom;

            if (i < _slotsArray.Length)
            {
                element.Text = prefix + _slotsArray[i].Name;
                action.Param = _slotsArray[i].Name;
            }
            else
            {
                element.Text = emptyName;
                action.Param = EMPTY_SLOT;
            }

            element.Action = action;
            this.Elements[i] = element;
        }

        if (this.Mode == MenuMode.Loading)
        {
            MenuElement eraseElement = new MenuElement();
            Action action = new Action();
            eraseElement.Text = "Erase Slots";
            action.Type = ActionType.Custom;
            action.Param = ERASE;
            eraseElement.Action = action;
            this.Elements[this.Elements.Length - 1] = eraseElement;
        }
    }

    public SaveSlotData.SlotSummary SaveSlotForName(string name)
    {
        return _slots[name];
    }

    public void CreateNewSlot()
    {
        SaveData.LoadFromDisk(SaveSlotData.CreateNewSlotName(_slotsArray));
    }

    /**
     * Private
     */
    Dictionary<string, SaveSlotData.SlotSummary> _slots;
    SaveSlotData.SlotSummary[] _slotsArray;
}
