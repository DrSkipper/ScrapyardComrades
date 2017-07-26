﻿using UnityEngine;
using System.Collections.Generic;

public static class SaveData
{
    public const string DEBUG_SLOT_NAME = "debug";
    public const string DATA_PATH = "saveslots/";
    public const string FILE_SUFFIX = ".dat";

    public static bool DataLoaded { get { return _loadedDiskData != null; } }
    public static string LoadedSaveSlotName { get { return _loadedDiskData.SaveSlotName; } }
    public static PlayerStatsModel PlayerStats { get { return _loadedDiskData.PlayerStats; } }

    public static EntityModel GetTrackedEntity(string quadName, string entityName)
    {
        Dictionary<string, EntityModel> quadEntities;
        if (!_loadedDiskData.TrackedEntities.ContainsKey(quadName))
        {
            quadEntities = new Dictionary<string, EntityModel>();
            _loadedDiskData.TrackedEntities.Add(quadName, quadEntities);
        }
        else
        {
            quadEntities = _loadedDiskData.TrackedEntities[quadName];
        }

        EntityModel entity;
        if (!quadEntities.ContainsKey(entityName))
        {
            entity = new EntityModel(quadName, entityName);
            quadEntities.Add(entityName, entity);
        }
        else
        {
            entity = quadEntities[entityName];
        }
        return entity;
    }
    
    public static bool CheckGlobalState(string stateName, string checkStateTag)
    {
        return _loadedDiskData.GlobalStates.ContainsKey(stateName) ? (_loadedDiskData.GlobalStates[stateName] == checkStateTag) : false;
    }

    public static void SetGlobalState(string stateName, string stateTag)
    {
        if (!_loadedDiskData.GlobalStates.ContainsKey(stateName))
            _loadedDiskData.GlobalStates.Add(stateName, stateTag);
        else
            _loadedDiskData.GlobalStates[stateName] = stateTag;
    }

    [System.Serializable]
    public class PlayerStatsModel
    {
        public int Level;
        public int MaxHealth;
        public int CurrentHealth;
    }

    [System.Serializable]
    public class EntityModel
    {
        public string QuadName;
        public string EntityName;
        public string StateTag;
        public bool Consumed;

        public EntityModel(string quadName, string entityName)
        {
            this.QuadName = quadName;
            this.EntityName = entityName;
            this.StateTag = null;
            this.Consumed = false;
        }
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
        public PlayerStatsModel PlayerStats;
        public Dictionary<string, Dictionary<string, EntityModel>> TrackedEntities;
        public Dictionary<string, string> GlobalStates;
    }

    /**
     * Private
     */
    private static DiskData _loadedDiskData;

    private static DiskData loadInitialSaveData()
    {
        DiskData data = new DiskData();
        data.PlayerStats = new PlayerStatsModel();
        data.TrackedEntities = new Dictionary<string, Dictionary<string, EntityModel>>();
        data.GlobalStates = new Dictionary<string, string>();
        return data;
    }
}
