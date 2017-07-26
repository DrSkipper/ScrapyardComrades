using UnityEngine;

public static class SaveSlotData
{
    public struct SlotSummary
    {
        public string Name;
        public long TimestampTicks;
        //public int PlayerLevel;
        //public string QuadName;

        public SlotSummary(SaveData.DiskData data)
        {
            this.Name = data.SaveSlotName;
            this.TimestampTicks = data.TimestampTicks;
            //this.NumHearts = data.HeroData.NumHearts;
            //this.Gold = data.HeroData.Gold;
            //this.Age = data.HeroData.Age;
        }
    }

    public static string CreateNameForSlotIndex(int index)
    {
        return SLOT_PREFIX + index;
    }

    public static SlotSummary[] GetAllSlots()
    {
        string[] saveSlotNames = DiskDataHandler.GetAllFilesAtPath(SaveData.DATA_PATH);

        SlotSummary[] slots = new SlotSummary[saveSlotNames.Length];
        for (int i = 0; i < saveSlotNames.Length; ++i)
        {
            slots[i] = slotSummaryForSlotName(saveSlotNames[i]);
        }
        return slots;
    }

    /**
     * Private
     */
    private const string SLOT_PREFIX = "SLOT_";

    private static SlotSummary slotSummaryForSlotName(string slotName)
    {
        int separator = slotName.LastIndexOf('.');
        if (separator > 0)
            slotName = slotName.Substring(0, separator);

        SaveData.DiskData slotData = DiskDataHandler.Load<SaveData.DiskData>(SaveData.DATA_PATH + slotName + SaveData.FILE_SUFFIX);
        
        return slotData != null ? new SlotSummary(slotData) : new SlotSummary();
    }
}
