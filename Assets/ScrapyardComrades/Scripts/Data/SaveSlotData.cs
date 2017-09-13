using UnityEngine;
using System.Collections.Generic;

public static class SaveSlotData
{
    public const int MAX_SLOTS = 5;

    public struct SlotSummary
    {
        public string Name;
        public bool UnsafeSave;
        public long TimestampTicks;
        public long GameplayTicks;
        public int PlayerLevel;
        public string LastSaveRoom;

        public SlotSummary(SaveData.DiskData data)
        {
            this.Name = data.SaveSlotName;
            this.UnsafeSave = data.UnsafeSave;
            this.TimestampTicks = data.TimestampTicks;
            this.GameplayTicks = data.GametimeTicks;
            this.PlayerLevel = data.PlayerStats.Level;
            this.LastSaveRoom = data.LastSaveRoom;
        }
    }

    public static string CreateNewSlotName(SlotSummary[] slots)
    {
        string name = SaveData.DEBUG_SLOT_NAME;
        for (int i = 0; i < MAX_SLOTS; ++i)
        {
            string n = SLOT_PREFIX + i;
            bool found = false;
            for (int j = 0; j < slots.Length; ++j)
            {
                if (slots[j].Name == n)
                {
                    found = true;
                }
            }
            if (!found)
            {
                name = n;
                break;
            }
        }
        return name;
    }

    public static SlotSummary[] GetAllSlots()
    {
        string[] saveSlotNames = DiskDataHandler.GetAllFilesAtPath(SaveData.DATA_PATH);

        List<SlotSummary> slots = new List<SlotSummary>(saveSlotNames.Length);
        for (int i = 0; i < saveSlotNames.Length; ++i)
        {
            slots.Add(slotSummaryForSlotName(saveSlotNames[i]));
        }
        slots.Sort(compareSlots);
        return slots.ToArray();
    }

    public static void EraseSlot(string slotName)
    {
        DiskDataHandler.Erase(SaveData.DATA_PATH + slotName + SaveData.FILE_SUFFIX);
    }

    /**
     * Private
     */
    private const string SLOT_PREFIX = "SLOT_";

    private static int compareSlots(SlotSummary a, SlotSummary b)
    {
        if (b.TimestampTicks < a.TimestampTicks)
            return -1;
        if (b.TimestampTicks > a.TimestampTicks)
            return 1;
        return 0;
    }

    private static SlotSummary slotSummaryForSlotName(string slotName)
    {
        int separator = slotName.LastIndexOf('.');
        if (separator > 0)
            slotName = slotName.Substring(0, separator);

        SaveData.DiskData slotData = DiskDataHandler.Load<SaveData.DiskData>(SaveData.DATA_PATH + slotName + SaveData.FILE_SUFFIX);
        
        return slotData != null ? new SlotSummary(slotData) : new SlotSummary();
    }
}
