using UnityEngine;
using System.Collections.Generic;

public class UISaveSlotMenu : Menu
{
    public const string EMPTY_SLOT = "empty";
    public const string EMPTY_SLOT_NAME = "Start New Game";

    void Awake()
    {
        _slots = new Dictionary<string, SaveSlotData.SlotSummary>();
        _slotsArray = SaveSlotData.GetAllSlots();

        for (int i = 0; i < _slotsArray.Length; ++i)
        {
            _slots[_slotsArray[i].Name] = _slotsArray[i];
        }

        this.Elements = new MenuElement[SaveSlotData.MAX_SLOTS];

        for (int i = 0; i < SaveSlotData.MAX_SLOTS; ++i)
        {
            MenuElement element = new MenuElement();
            Action action = new Action();
            action.Type = ActionType.Custom;

            if (i < _slotsArray.Length)
            {
                element.Text = _slotsArray[i].Name;
                action.Param = _slotsArray[i].Name;
            }
            else
            {
                element.Text = EMPTY_SLOT_NAME;
                action.Param = EMPTY_SLOT;
            }

            element.Action = action;
            this.Elements[i] = element;
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
