using UnityEngine;
using System.Collections.Generic;

public static class SaveData
{
    public const string DEBUG_SLOT_NAME = "debug";
    public const string DATA_PATH = "saveslots/";
    public const string FILE_SUFFIX = ".dat";

    public static bool DataLoaded { get { return _loadedDiskData != null; } }
    public static string LoadedSaveSlotName { get { return _loadedDiskData.SaveSlotName; } }
    public static PlayerStats PlayerData { get { return _loadedDiskData.PlayerData; } }
    public static Dictionary<string, Dictionary<string, EntityTracker.Entity>> TrackedEntities { get { return _loadedDiskData.TrackedEntities; } }
    public static Dictionary<string, string> GlobalStates { get { return _loadedDiskData.GlobalStates; } }
    
    [System.Serializable]
    public class PlayerStats
    {
        public int Level;
        public int MaxHealth;
        public int CurrentHealth;
    }
    
    public static void LoadFromDisk(string slotName)
    {
        string path = DATA_PATH + slotName + FILE_SUFFIX;
        if (DiskDataHandler.FileExists(path))
        {
            _loadedDiskData = DiskDataHandler.Load<DiskData>(path);
        }
        else
        {
            _loadedDiskData = loadInitialSaveData();
            _loadedDiskData.SaveSlotName = slotName;
        }
    }

    public static void SaveToDisk(string overrideSaveName = null)
    {
        // Don't save the debug slot to disk
        if ((!StringExtensions.IsEmpty(overrideSaveName) && overrideSaveName == DEBUG_SLOT_NAME) || _loadedDiskData.SaveSlotName == DEBUG_SLOT_NAME)
            return;

        DiskDataHandler.GuaranteeDirectoryExists(DATA_PATH);
        _loadedDiskData.TimestampTicks = System.DateTime.Now.Ticks;
        DiskDataHandler.Save(DATA_PATH + _loadedDiskData.SaveSlotName + FILE_SUFFIX, _loadedDiskData);
    }

    public static void WipeLoadedData()
    {
        _loadedDiskData = null;
    }

    public static void EraseSaveSlot(string slotName)
    {
        DiskDataHandler.Erase(DATA_PATH + slotName + FILE_SUFFIX);
    }

    public class DiskData
    {
        public string SaveSlotName;
        public long TimestampTicks;
        public PlayerStats PlayerData;
        public Dictionary<string, Dictionary<string, EntityTracker.Entity>> TrackedEntities;
        public Dictionary<string, string> GlobalStates;
    }

    /**
     * Private
     */
    private static DiskData _loadedDiskData;

    private static DiskData loadInitialSaveData()
    {
        DiskData data = new DiskData();
        data.PlayerData = new PlayerStats();
        data.TrackedEntities = new Dictionary<string, Dictionary<string, EntityTracker.Entity>>();
        data.GlobalStates = new Dictionary<string, string>();
        return data;
    }
}
